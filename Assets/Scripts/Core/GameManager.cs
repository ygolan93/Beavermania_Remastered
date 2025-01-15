using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Beavermania.Core.Input;
using Beavermania.Display;

namespace Beavermania.Core.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private GameObject pauseMenu;
        //private bool _isPaused = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _input.PauseEvent += HandlePause;
            _input.ResumeEvent += HandleResume;


        }
        private void HandlePause() 
        {
            pauseMenu.SetActive(true);
        }
        private void HandleResume() 
        {
            pauseMenu.SetActive(false);
        }
    }
}


