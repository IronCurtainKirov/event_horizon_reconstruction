using Combat.Manager;
using Services.Gui;
using Services.Messenger;
using UnityEngine;
using Zenject;


namespace Gui.Combat
{
    public class MapVariableButton : MonoBehaviour
    {
        public void Open()
        {
            GetComponent<IWindow>().Open();
        }

        public void Close()
        {
            GetComponent<IWindow>().Close();
        }

    }
}
