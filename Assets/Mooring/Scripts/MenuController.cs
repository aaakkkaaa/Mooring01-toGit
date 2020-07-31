using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RootMotion.FinalIK;
using Crest;

public class MenuController : MonoBehaviour
{
    // нужно ли грузить сразу сцену, до выбора в меню
    public bool LoadDefaultScene = true;
    public string DefaultLocationScene;

    // текущая загруженная сцена
    private string _scene = "";

    // меню
    private GameObject _menuCanvas;
    private GameObject _panelBtn;
    private GameObject _panelUI;
    private GameObject _btnLesson1;

    private Slider _windValue;
    private Text _windValTxt;
    private Slider _windDir;
    private Text _windDirTxt;
    private Slider _waveVolume;
    private Text _waveVolTxt;

    // настройка ветра
    private Wind _wind;
    // настройка волн
    private OceanWaveSpectrum _waveSpect;

    // скрипт инверсной кинематики Кадета, в нем данные калибровки
    private VRIK _vrik;
    private GameObject _cadet;

    // стойка руля
    private GameObject _stand;

    // параметры калибровки которые надо сохранить и восстановить при перезагрузке сцены
    private bool FirstLoad = true;
    private Vector3 _leftHandTrackerTargetLocPos;
    private Vector3 _leftHandTrackerTargetLocEu;
    private Vector3 _rightHandTrackerTargetLocPos;
    private Vector3 _rightHandTrackerTargetLocEu;
    private Vector3 _cadetLocScale;

    private Vector3 _standLocalPos;

    private Vector3 _cameraRIGLocPos;


    private void Awake()
    {
        // найдем компоненты UI
        _menuCanvas = transform.Find("Canvas").gameObject;

        _panelBtn = GameObject.Find("MainMenu/Canvas/PanelBtn");
        _panelUI = GameObject.Find("MainMenu/Canvas/PanelUI");

        _btnLesson1 = GameObject.Find("MainMenu/Canvas/PanelBtn/Lesson1");
        _btnLesson1.GetComponent<Button>().onClick.AddListener(LoadLesson1);

        _windValue = GameObject.Find("MainMenu/Canvas/PanelUI/WindValue").GetComponent<Slider>();
        _windValTxt = GameObject.Find("MainMenu/Canvas/PanelUI/WindValueTxt").GetComponent<Text>();

        _windDir = GameObject.Find("MainMenu/Canvas/PanelUI/WindDirection").GetComponent<Slider>();
        _windDirTxt = GameObject.Find("MainMenu/Canvas/PanelUI/WindDirTxt").GetComponent<Text>();

        _waveVolume = GameObject.Find("MainMenu/Canvas/PanelUI/WaveVolume").GetComponent<Slider>();
        _waveVolTxt = GameObject.Find("MainMenu/Canvas/PanelUI/WaveVolumeTxt").GetComponent<Text>();

    }

    private void Start()
    {
        // чтобы не восстанавливать параметры калибровки при первом запуске
        FirstLoad = true;

        // проверка, загружены ли уже сцены локации и урока
        _stand = GameObject.Find("Stand");
        if(_stand != null)
        {
            print("Сцены локации и урока загружены в редакторе!");
            _scene = _stand.scene.name;
            FindObjects();
            UISetup();
        }
        else
        {
            print("Загрузка сцены локации и урока по умолчанию");
            if(LoadDefaultScene && DefaultLocationScene != "")
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadSceneAsync(DefaultLocationScene, LoadSceneMode.Additive);
                _scene = DefaultLocationScene;
            }
        }

        // скроем меню
        _menuCanvas.SetActive(false);
        
    }

    // найти в иерархии рабочие объекты (это нужно делать после каждой загрузки сцены)
    private void FindObjects()
    {
        GameObject windField = GameObject.Find("WindField");
        _wind = windField.GetComponent<Wind>();
        _cadet = GameObject.Find("Cadet");
        _vrik = _cadet.GetComponent<VRIK>();

        _stand = GameObject.Find("Stand");

        GameObject waves = GameObject.Find("Environment/Waves");
        ShapeGerstnerBatched shapeGer = waves.GetComponent<ShapeGerstnerBatched>();
        _waveSpect = shapeGer._spectrum;

        // Установить начальное значение волнения
        _waveSpect._multiplier = 0.1f;
    }

    void Update()
    {
        // спрятать -- показать меню
        if (Input.GetKey("left ctrl") || Input.GetKey("right ctrl"))
        {
            if (Input.GetKeyDown("c"))
            {
                if (_menuCanvas.activeSelf)
                {
                    _menuCanvas.SetActive(false);
                }
                else
                {
                    _menuCanvas.SetActive(true);
                    // отобразить в UI текущие значения переменных
                    UISetup();

                }
            }
        }
    }

    // нажата кнопка загрузки сцены
    private void LoadLesson1()
    {
        print("LoadLesson1()");

        if(_scene != "")
        {
            // сохранить настройки калибровки
            СalibrationSave();

            SceneManager.UnloadSceneAsync(_scene);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadSceneAsync(DefaultLocationScene, LoadSceneMode.Additive);
    }

    // сохранить настройки калибровки
    private void СalibrationSave()
    {
        _leftHandTrackerTargetLocPos = _vrik.solver.leftArm.target.localPosition;
        _leftHandTrackerTargetLocEu = _vrik.solver.leftArm.target.localEulerAngles;
        _rightHandTrackerTargetLocPos = _vrik.solver.rightArm.target.localPosition;
        _rightHandTrackerTargetLocEu = _vrik.solver.rightArm.target.localEulerAngles;
        _cadetLocScale = _cadet.transform.localScale;

        _standLocalPos = _stand.transform.localPosition;

        _cameraRIGLocPos = _vrik.solver.leftArm.target.parent.parent.localPosition;

        print("Настройки калибровки сохранены");
    }

    // сцена загрузилась
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _scene = scene.name;

        print("OnSceneLoaded: " + scene.name + "    LoadSceneMode: " + mode);

        // найти объекты, которые пересоздались при загрузке сцены
        FindObjects();

        // Настроить UI в соответсвии с заново загруженнной сценой
        UISetup();

        // восстановить настройки калибровки, если это не первый запуск
        if ( !FirstLoad )
        {
            СalibrationReset();
        }
        FirstLoad = false;
    }

    // восстановление значений контролов
    private void UISetup()
    {
        if (_menuCanvas.activeSelf)
        {
            // если никакая сцена локации не загружена, органы UI не могут быть выставлены
            _stand = GameObject.Find("Stand");
            if (_stand != null)
            {
                _panelUI.SetActive(true);

                _windValue.value = _wind.WindDir[0].value;
                _windValTxt.text = string.Format("{0:F1}", _windValue.value);
                _windDir.value = _wind.WindDir[0].angle;
                _windDirTxt.text = string.Format("{0:F1}", _windDir.value);
                _waveVolume.value = _waveSpect._multiplier;
                _waveVolTxt.text = string.Format("{0:F1}", _waveVolume.value);
            }
            else
            {
                _panelUI.SetActive(false);
            }
        }
    }

    // восстановить настройки калибровки
    private void СalibrationReset()
    {
        _vrik.solver.leftArm.target.localPosition = _leftHandTrackerTargetLocPos;
        _vrik.solver.leftArm.target.localEulerAngles = _leftHandTrackerTargetLocEu;
        _vrik.solver.rightArm.target.localPosition = _rightHandTrackerTargetLocPos;
        _vrik.solver.rightArm.target.localEulerAngles = _rightHandTrackerTargetLocEu;
        _cadet.transform.localScale = _cadetLocScale;

        _stand.transform.localPosition = _standLocalPos;

        _vrik.solver.leftArm.target.parent.parent.localPosition = _cameraRIGLocPos;

        print("Настройки калибровки восстановлены");

    }


    public void onWindValueChange()
    {
        _wind.WindDir[0].value = _windValue.value;
        _windValTxt.text = string.Format("{0:F1}", _windValue.value);
    }

    public void onWindDirectChange()
    {
        _wind.WindDir[0].angle = _windDir.value;
        _windDirTxt.text = string.Format("{0:F1}", _windDir.value);
    }

    public void onWaveVolumeChange()
    {
        print("_waveSpect._multiplier = " + _waveSpect._multiplier);
        _waveSpect._multiplier = _waveVolume.value;
        print("_waveSpect._multiplier = " + _waveSpect._multiplier);
        _waveVolTxt.text = string.Format("{0:F1}", _waveVolume.value);
    }

}
