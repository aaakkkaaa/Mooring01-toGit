using System.Globalization;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.Windows;
using TMPro;

public class KeywordScript : MonoBehaviour
{
    [SerializeField]
    private string[] m_Keywords;

    private KeywordRecognizer m_Recognizer;

    [SerializeField]
    private Sailor _Sailor1;


    void Start()
    {
        //var language = new Windows.Globalization.Language("en-US");
        //var recognizer = new SpeechRecognizer(language);

        System.Globalization.CultureInfo.CurrentCulture = new CultureInfo("ru-RU", false);



        m_Recognizer = new KeywordRecognizer(m_Keywords);
        m_Recognizer.OnPhraseRecognized += OnPhraseRecognized;
        m_Recognizer.Start();

        var keywords = m_Recognizer.Keywords;
        foreach (string word in keywords)
        {
            Debug.Log(word);
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        Debug.Log(builder.ToString());

        if (_Sailor1 != null)
        {
            if (_Sailor1.CurCommand == "")
            {
                switch (args.text)
                {
                    case "forecastle":
                        _Sailor1.CurCommand = "НА НОС";
                        break;
                    case "give the stern line":
                    case "give the stern rope":
                    case "give the aft line":
                    case "give the aft rope":
                        _Sailor1.CurCommand = "ПОДАТЬ ШВАРТОВЫ";
                        break;
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (m_Recognizer != null && m_Recognizer.IsRunning)
        {
            m_Recognizer.OnPhraseRecognized -= OnPhraseRecognized;
            m_Recognizer.Stop();
        }
    }

}