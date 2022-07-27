using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WrenSharp;
using WrenSharp.Unity;

public class WrenREPL : MonoBehaviour, IWrenWriteOutput, IWrenErrorOutput
{
    private const int MaxOutputLength = 4096;
    private const int MaxHistoryLength = 20;

    private UnityWrenVM m_VM;
    private StringBuilder m_WriteBuffer;
    private StringBuilder m_OutputBuffer;
    private string[] m_History;
    private int m_HistoryLength;
    private int m_HistoryCurrent;
    private int m_HistoryTail;

    [SerializeField] private InputField m_Input = default;
    [SerializeField] private Text m_Output = default;
    [Space]
    [SerializeField] private Button m_ButtonClear = default;
    [SerializeField] private Button m_ButtonReset = default;
    [Space]
    [SerializeField, TextArea] private string m_InitialInput = default;

    #region MonoBehaviour

    private void Awake()
    {
        m_WriteBuffer = new StringBuilder(256);
        m_OutputBuffer = new StringBuilder(MaxOutputLength);
        m_History = new string[MaxHistoryLength];
        m_HistoryLength = 0;
        m_HistoryCurrent = 0;
        m_HistoryTail = 0;

        ResetVM();

        m_ButtonClear.onClick.AddListener(() => ClearOutput());
        m_ButtonReset.onClick.AddListener(() => ResetVM());

        m_Input.onSubmit.AddListener((input) => SubmitInput(input));
        m_Input.text = m_InitialInput;
    }

    private void OnDestroy()
    {
        m_VM.Dispose();
    }

    private void Update()
    {
        ProcessInput();
    }

    #endregion

    private void ResetVM()
    {
        m_VM?.Dispose();
        m_VM = new UnityWrenVM(new WrenVMConfiguration()
        {
            WriteOutput = this,
            ErrorOutput = this,
        });

        ClearOutput();
        ClearInput();
    }

    private void ProcessInput()
    {
        if (!m_Input.isFocused)
            return;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                HistoryNavigate(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                HistoryNavigate(1);
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                SubmitInput(m_Input.text);
            }
        }
    }

    private void SubmitInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return;

        m_VM.Interpret("repl", input);

        FlushWriteBuffer();
        AppendHistory(input);
        ClearInput();
    }

    private void FlushWriteBuffer()
    {
        if (m_WriteBuffer.Length <= 0)
            return;

        if (m_WriteBuffer[m_WriteBuffer.Length - 1] == '\n')
        {
            m_WriteBuffer.Length--;
        }

        Output(m_WriteBuffer.ToString(), LogType.Log);
        m_WriteBuffer.Clear();
    }

    private void Output(string text, LogType logType)
    {
        if (string.IsNullOrEmpty(text))
            return;

        int newLen = m_OutputBuffer.Length + text.Length;
        if (newLen > MaxOutputLength)
        {
            m_OutputBuffer.Remove(0, newLen - MaxOutputLength);
        }

        m_OutputBuffer.AppendLine(text);
        m_Output.text = m_OutputBuffer.ToString();

        // Forward to logger as well
        Debug.unityLogger.Log(logType, text);
    }

    private void ClearInput()
    {
        m_Input.text = string.Empty;
    }

    private void ClearOutput()
    {
        m_OutputBuffer.Clear();
        m_Output.text = string.Empty;
    }

    private void AppendHistory(string text)
    {
        m_History[m_HistoryTail] = text;
        
        if (m_HistoryLength < MaxHistoryLength)
        {
            m_HistoryLength++;
        }

        m_HistoryTail = (m_HistoryTail + 1) % MaxHistoryLength;
        m_HistoryCurrent = m_HistoryTail;
    }

    private void HistoryNavigate(int dir)
    {
        if (m_HistoryLength == 0)
            return;

        m_HistoryCurrent += dir;

        if (dir > 0)
        {
            if (m_HistoryCurrent >= m_HistoryTail)
            {
                m_HistoryCurrent = m_HistoryTail;
                m_Input.text = string.Empty;
                return;
            }
        }
        else
        {
            if (m_HistoryCurrent < 0)
            {
                if (m_HistoryLength < MaxHistoryLength)
                {
                    m_HistoryCurrent = 0;
                    return;
                }

                m_HistoryCurrent = MaxHistoryLength - 1;
            }
            
            if (m_HistoryCurrent == m_HistoryTail)
                return;
        }

        m_Input.text = m_History[m_HistoryCurrent];
        m_Input.caretPosition = 0;
    }


    void IWrenWriteOutput.OutputWrite(WrenVM vm, string text)
    {
        m_WriteBuffer.Append(text);
    }

    void IWrenErrorOutput.OutputError(WrenVM vm, WrenErrorType errorType, string module, int lineNumber, string message)
    {
        switch (errorType)
        {
            case WrenErrorType.Compile:
                Output($"[{module}: ln {lineNumber}] [Error] {message}", LogType.Error);
                break;

            case WrenErrorType.StackTrace:
                Output($"[{module}: ln {lineNumber}] in {message}", LogType.Error);
                break;

            case WrenErrorType.Runtime:
                Output($"[Error] {message}", LogType.Error);
                break;
        }
    }
}
