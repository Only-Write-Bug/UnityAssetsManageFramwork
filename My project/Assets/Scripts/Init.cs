using System;
using Tools;
using UnityEngine;

namespace DefaultNamespace
{
    public class Init : MonoBehaviour
    {
        private void Start()
        {
            ExcelReader.init.LoadAllExcels();
        }
    }
}