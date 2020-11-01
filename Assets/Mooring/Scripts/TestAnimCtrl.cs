using UnityEngine;

public class TestAnimCtrl : MonoBehaviour
{
    // Трансформ матроски
    [SerializeField]
    Transform _SailorF;

    // Компонент матроски Animator
    [SerializeField]
    Animator _Animator;

    // Старый AnimatorController
    [SerializeField]
    RuntimeAnimatorController _AnimatorController1;

    // Новый AnimatorController
    [SerializeField]
    RuntimeAnimatorController _AnimatorController2;

    // Start is called before the first frame update
    void Start()
    {
        // Трансформ матроски
        _SailorF = GameObject.Find("SailorF2").transform;
        // Компонент матроски Animator
        _Animator = _SailorF.GetComponent<Animator>();
        // AnimatorController аниматора матроски, установленный в редакторе.
        _AnimatorController1 = _Animator.runtimeAnimatorController;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("1"))
        {

            //print("1 pressed");
            // Установить старый AnimatorController
            _Animator.runtimeAnimatorController = _AnimatorController1;
        }
        else if (Input.GetKeyDown("2"))
        {
            //print("2 pressed");
            // Установить новый AnimatorController
            _Animator.runtimeAnimatorController = _AnimatorController2;
        }
    }
}
