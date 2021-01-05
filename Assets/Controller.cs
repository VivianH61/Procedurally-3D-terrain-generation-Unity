using UnityEngine;
using System.Collections;

namespace Gavin
{
    public class Controller : MonoBehaviour
    {
        //旋转变量;
        private float m_deltX = 0f;
        private float m_deltY = 0f;
        //缩放变量;
        private float m_distance = 100f;
        private float m_mSpeed = 5f;
        //移动变量;
        private Vector3 m_mouseMovePos = Vector3.zero;
        //移动速度
        private float cameraSpeed = 1f;

        void Start()
        {
            GetComponent<Camera>().transform.localPosition = new Vector3(0, m_distance, 0);
        }

        void Update()
        {
            //鼠标右键点下控制相机旋转;
            if (Input.GetMouseButton(1))
            {
                m_deltX += Input.GetAxis("Mouse X") * m_mSpeed;
                m_deltY -= Input.GetAxis("Mouse Y") * m_mSpeed;
                m_deltX = ClampAngle(m_deltX, -360, 360);
                m_deltY = ClampAngle(m_deltY, -70, 70);
                GetComponent<Camera>().transform.rotation = Quaternion.Euler(m_deltY, m_deltX, 0);
            }

            if (Input.GetKey(KeyCode.W))
            {
                GetComponent<Camera>().transform.Translate(new Vector3(0, 0, cameraSpeed), Space.Self);
            }

            if (Input.GetKey(KeyCode.S))
            {
                GetComponent<Camera>().transform.Translate(new Vector3(0, 0, -cameraSpeed), Space.Self);
            }

            if (Input.GetKey(KeyCode.A))
            {
                GetComponent<Camera>().transform.Translate(new Vector3(-cameraSpeed, 0, 0), Space.Self);
            }

            if (Input.GetKey(KeyCode.D))
            {
                GetComponent<Camera>().transform.Translate(new Vector3(cameraSpeed, 0, 0), Space.Self);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                GetComponent<Camera>().transform.Translate(new Vector3(0, -cameraSpeed, 0), Space.Self);
            }

            if (Input.GetKey(KeyCode.E))
            {
                GetComponent<Camera>().transform.Translate(new Vector3(0, cameraSpeed, 0), Space.Self);
            }
        }

        //规划角度;
        float ClampAngle(float angle, float minAngle, float maxAgnle)
        {
            if (angle <= -360)
                angle += 360;
            if (angle >= 360)
                angle -= 360;

            return Mathf.Clamp(angle, minAngle, maxAgnle);
        }
    }
}