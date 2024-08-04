using System;
using Tools;
using UnityEngine;

namespace DefaultNamespace
{
    public class Init : MonoBehaviour
    {
        private void Start()
        {
            InitBaseFeatures();
        }

        private void InitBaseFeatures()
        {
            ExcelReader.init.LoadAllExcels();
        }
    }
}