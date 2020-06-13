using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sBump : MonoBehaviour
{

    // Флаг наличия столкновения
    bool _Clash = false;

    // Словарь коллайдеров. Ключ - коллайдер, значение - флаг участия в коллизии
    Dictionary<Collider, bool> _AllColliders = new Dictionary<Collider, bool>();

    // Словарь столкновений. Ключ - ID чужого коллайдера + ID своего коллайдера, значение - флаг участия в коллизии
    Dictionary<string, bool> _AllCollisions = new Dictionary<string, bool>();


    // Аудио источники
    AudioSource _Impact; // Короткий удар
    AudioSource _Crash; // Удвр + стекло
    AudioSource _Scrape; //Скрежет

    // Start is called before the first frame update
    void Start()
    {
        //print("InstanceID of Game Object = " + gameObject.GetInstanceID());
        //print("InstanceID of Transform = " + gameObject.transform.GetInstanceID());
        //print("InstanceID of Rigidbody = " + gameObject.GetComponent<Collider>().GetInstanceID());

        //// Собрать в словарь все коллайдеры
        //getChildColliders(transform);

        //print("Собрали всего коллайдеров: " + _AllColliders.Count);

        // Аудио источники
        _Impact = GameObject.Find("Impact").GetComponent<AudioSource>();
        _Crash = GameObject.Find("Crash").GetComponent<AudioSource>();
        _Scrape = GameObject.Find("Scrape").GetComponent<AudioSource>();

    }

    // Рекурсивная функция - обойти все дочерние объекты и собрать их коллайдеры в общий словарь
    private void getChildColliders(Transform myObject)
    {
        int ChildCount = myObject.childCount;
        for (int i = 0; i < ChildCount; i++)
        {
            Transform myChild = myObject.GetChild(i);
            foreach (Collider myColl in myChild.GetComponents<Collider>())
            {
                myColl.contactOffset = 0.001f;
                _AllColliders.Add(myColl, false);
            }
            getChildColliders(myChild); // Рекурсия
        }
    }


    // Вошли в коллизию
    void OnCollisionEnter(Collision collision)
    {
        // Проверим, подходит ли внешний коллайдер по тэгу объекта
        if (CheckCollider(collision.gameObject.tag))
        {
            // Массив точек контактов
            ContactPoint[] ContactPoints = new ContactPoint[collision.contactCount];
            collision.GetContacts(ContactPoints);

            _Impact.transform.position = ContactPoints[0].point;
            if (!_Impact.isPlaying)
            {
                _Impact.volume = Mathf.Clamp(collision.relativeVelocity.magnitude + 0.5f, 0, 5) / 3;
                _Impact.Play();
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Проверим, подходит ли внешний коллайдер по тэгу объекта
        if (CheckCollider(collision.gameObject.tag))
        {
            // Массив точек контактов
            ContactPoint[] ContactPoints = new ContactPoint[collision.contactCount];
            collision.GetContacts(ContactPoints);

            _Scrape.transform.position = ContactPoints[0].point;
            if (!_Scrape.isPlaying)
            {
                _Scrape.Play();
            }
        }
    }


    // Вышли из коллизии
    void OnCollisionExit(Collision collision)
    {
        // Проверим, подходит ли внешний коллайдер по тэгу объекта
        if (CheckCollider(collision.gameObject.tag))
        {
            _Scrape.Stop();
        }

    }


    // Проверка внешнего коллайдера по тэгу
    private bool CheckCollider(string collTag)
    {
        if (collTag == "Pier" || collTag == "Ship")
        {
            return true;
        }
        return false;
    }


}
