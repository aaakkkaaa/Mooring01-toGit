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
        // Проверим, подходит ли внешний коллайдер по имени объекта
        string myName = collision.gameObject.name;

        if (myName.Length < 8 || myName.Substring(0, 8) != "PortFlor")
        {
            return;
        }

        // Массив точек контактов
        ContactPoint[] ContactPoints = new ContactPoint[collision.contactCount];
        collision.GetContacts(ContactPoints);

        _Impact.transform.position = ContactPoints[0].point;
        if (!_Impact.isPlaying)
        {
            _Impact.volume = Mathf.Clamp(collision.relativeVelocity.magnitude + 0.5f, 0, 5) / 5;
            _Impact.Play();
        }

    }

    void OnCollisionStay(Collision collision)
    {
        // Проверим, подходит ли внешний коллайдер по имени объекта
        string myName = collision.gameObject.name;
        if (myName.Length < 8 || myName.Substring(0, 8) != "PortFlor")
        {
            return;
        }

        // Массив точек контактов
        ContactPoint[] ContactPoints = new ContactPoint[collision.contactCount];
        collision.GetContacts(ContactPoints);

        _Scrape.transform.position = ContactPoints[0].point;
        if (!_Scrape.isPlaying)
        {
            _Scrape.Play();
        }
    }


    // Вышли из коллизии
    void OnCollisionExit(Collision collision)
    {
        // Проверим, подходит ли внешний коллайдер по имени объекта
        string myName = collision.gameObject.name;
        if (myName.Length < 8 || myName.Substring(0, 8) != "PortFlor")
        {
            return;
        }

        _Scrape.Stop();
    }





        // Вошли в коллизию
        void OnCollisionEnter1(Collision collision)
    {

        // Проверим, подходит ли внешний коллайдер по имени объекта
        string myName = collision.gameObject.name;
        if (myName.Length < 8 || myName.Substring(0, 8) != "PortFlor")
        {
            return;
        }

        print("");
        print("");
        //print("Есть контакт! Количество точек = " + collision.contactCount + ". Коллайдер: " + collision.collider + ". gameObject = " + collision.gameObject + ". impulse = " + collision.impulse);
        //print("RelativeVelocity = " + collision.relativeVelocity + ", magnitude = " + collision.relativeVelocity.magnitude);
        // Массив точек контактов
        ContactPoint[] ContactPoints = new ContactPoint[collision.contactCount];
        collision.GetContacts(ContactPoints);

        foreach (ContactPoint contact in ContactPoints)
        {
            //Debug.DrawRay(contact.point, contact.normal, Color.white);
            print("otherCollider = " + contact.otherCollider + ", thisCollider = " + contact.thisCollider + ", separation = " + contact.separation);

            // Запишем новую коллизию в словарь
            string myKey = contact.otherCollider.GetInstanceID().ToString() + contact.thisCollider.GetInstanceID().ToString();
            if (!_AllCollisions.ContainsKey(myKey))
            {
                _AllCollisions.Add(myKey, true);
            }

        }
        print("Есть контакт! Количество точек = " + collision.contactCount + " Всего коллизий: " + _AllCollisions.Count);

        if (!_Clash)
        {
            _Clash = true;
            _Impact.volume = Mathf.Clamp(collision.relativeVelocity.magnitude + 0.5f, 0, 5) / 5;
            _Impact.Play();

        }
    }

    // Находимся в коллизии
    void OnCollisionStay1(Collision collision)
    {
        // Проверим, подходит ли внешний коллайдер по имени объекта
        string myName = collision.gameObject.name;
        if (myName.Length < 8 || myName.Substring(0, 8) != "PortFlor")
        {
            return;
        }
        if (!_Scrape.isPlaying)
        {
            _Scrape.Play();
        }
    }

    // Вышли из коллизии
    void OnCollisionExit1(Collision collision)
    {
        // Проверим, подходит ли внешний коллайдер по имени объекта
        string myName = collision.gameObject.name;
        if (myName.Length < 8 || myName.Substring(0, 8) != "PortFlor")
        {
            return;
        }

        // Массив точек контактов
        ContactPoint[] ContactPoints = new ContactPoint[collision.contactCount];
        collision.GetContacts(ContactPoints);

        foreach (ContactPoint contact in ContactPoints)
        {
            // Удалим коллизии из словаря
            string myKey = contact.otherCollider.GetInstanceID().ToString() + contact.thisCollider.GetInstanceID().ToString();
            _AllCollisions.Remove(myKey);
        }

        print("Вышли из контакта. Осталось коллизий: " + _AllCollisions.Count);

        if (_AllCollisions.Count == 0)
        {
            _Clash = false;
            _Scrape.Stop();
        }
    }




}
