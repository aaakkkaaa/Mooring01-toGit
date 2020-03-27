using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class RopeSave : MonoBehaviour
{
    private bool showGUI = false;       // показывать ли интерфейс для сохранения/загрузки

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown("s"))
        {
            showGUI = !showGUI;
        }
    }

    // для отладки - интерфейс для загрузки и сохранения в xml
    string _fName;
    void OnGUI()
    {
        if (showGUI)
        {
            _fName = GUI.TextField(new Rect(10, 10, 100, 20), _fName);
            if (GUI.Button(new Rect(10, 40, 100, 30), "Load"))
            {
                LoadRope();
            }
            if (GUI.Button(new Rect(10, 70, 100, 30), "Save"))
            {
                SaveRope();
            }
        }
    }

    private void SaveRope()
    {
        // объект для сериализации
        RopeSettings rs = new RopeSettings();

        // получить параметры точек из Solver
        ObiRope obiR = gameObject.GetComponent<ObiRope>();
        ObiSolver obiS = obiR.solver;

        rs.pos = new Vector3[obiR.particleCount];
        rs.vel = new Vector3[obiR.particleCount];
        for (int i = 0; i < obiR.particleCount; i++)
        {
            int solverIndex = obiR.solverIndices[i];
            rs.pos[i] = obiS.positions[solverIndex];
            rs.vel[i] = obiS.velocities[solverIndex];
        }

        // Еще нужно сохранять параметры объектов, скрепленных с канатом
        ObiParticleAttachment[] allAt = gameObject.GetComponents<ObiParticleAttachment>();
        Vector3[] atPos = new Vector3[allAt.Length];
        Vector3[] atRot = new Vector3[allAt.Length];
        for(int i=0; i<allAt.Length; i++)
        {
            atPos[i] = allAt[i].target.position;
            atRot[i] = allAt[i].target.eulerAngles;
        }
        rs.attachPos = atPos;
        rs.attachRot = atRot;

        // сериализуем 
        string settingPath = Path.Combine(Directory.GetCurrentDirectory(), _fName);
        XmlSerializer myXmlSrlzr = new XmlSerializer(typeof(RopeSettings));
        XmlWriterSettings myXmlSettings = new XmlWriterSettings();
        myXmlSettings.Encoding = System.Text.Encoding.UTF8;
        myXmlSettings.Indent = true;
        using (XmlWriter myXmlWrtr = XmlWriter.Create(settingPath, myXmlSettings))
        {
            myXmlSrlzr.Serialize(myXmlWrtr, rs);
        }
    }

    private void LoadRope()
    {
        // считываем из файла
        string settingPath = Path.Combine(Directory.GetCurrentDirectory(), _fName);
        XmlSerializer myXmlSrlzr = new XmlSerializer(typeof(RopeSettings));
        RopeSettings rs;
        using (XmlReader myXmlRdr = XmlReader.Create(settingPath))
        {
            rs = myXmlSrlzr.Deserialize(myXmlRdr) as RopeSettings;
        }

        // Находим компоненты
        ObiRope obiR = gameObject.GetComponent<ObiRope>();
        ObiSolver obiS = obiR.solver;

        // устанавливаем параметры Rope
        for (int i = 0; i < obiR.particleCount; i++)
        {
            int solverIndex = obiR.solverIndices[i];
            obiS.positions[solverIndex] = rs.pos[i];
            obiS.velocities[solverIndex] = rs.vel[i];
        }

        // устанавливаем параматры связанных объектов
        ObiParticleAttachment[] allAt = gameObject.GetComponents<ObiParticleAttachment>();
        for (int i = 0; i < allAt.Length; i++)
        {
            allAt[i].target.position = rs.attachPos[i];
            allAt[i].target.eulerAngles = rs.attachRot[i];
        }

    }


    [Serializable]
    public class RopeSettings
    {
        public RopeSettings() { }
        public Vector3[] pos;
        public Vector3[] vel;
        public Vector3[] attachPos;
        public Vector3[] attachRot;
    }


}
