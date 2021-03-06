﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Core.Characters;

namespace AI {
    public class Agent : MonoBehaviour {

        private Vector3 destination;
        private List<Vector3> path;
        private float lastUpdateTime = -10;
        private float delta;
        private bool stopped = false;

        public GameObject Player;

        // Use this for initialization
        void Start() {
            delta = ServiceLocator.Instance.ResolveService<GameSettingsProvider>()
                .GetSettings().PathfindingUpdateDelta;
            //transform.transform.parent = null;
        }

        /// <summary>
        /// Идёт по пути
        /// </summary>
        /// <returns></returns>
        private IEnumerator FollowPath()
        {
            int currentIndex = 0;
            SetRotation(path[currentIndex]);
            while (transform.position != path[path.Count - 1])
            {
                if (stopped)
                    yield break;

                var contr = ServiceLocator.Instance.ResolveSingleton<CharactersController>();
                if (Time.time > contr.ConfusedTo)
                {
                    if (path[currentIndex] == transform.position)
                    {
                        currentIndex++;
                        SetRotation(path[currentIndex]);
                    }
                    transform.position = Vector3.MoveTowards(transform.position, path[currentIndex],
                        Speed * Time.deltaTime);
                }
                yield return null;
            }
        }

        /// <summary>
        /// Пытается идти
        /// </summary>
        /// <returns></returns>
        private IEnumerator TryPath()
        {
            while (!stopped)
            {
                transform.Translate((Destination - transform.position) * Speed * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>
        /// Поворачивает агента
        /// </summary>
        /// <param name="point">куда смотреть</param>
        private void SetRotation(Vector3 point)
        {

            Vector3 direction = (point - transform.position).normalized;

            var contr = ServiceLocator.Instance.ResolveSingleton<CharactersController>();
            if(Time.time < contr.ConfusedTo)
                direction = new Vector3(Random.Range(-2,2)+0.1f, Random.Range(-2, 2), 0).normalized;

            Vector3 rot = new Vector3();
            if (direction.y > 0)
                rot.z = Mathf.Acos(direction.x) * Mathf.Rad2Deg;
            else
                rot.z = -Mathf.Acos(direction.x) * Mathf.Rad2Deg;
            rot.z += 270;
            transform.eulerAngles = rot;
        }

        /// <summary>
        /// Скорость
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Направление
        /// </summary>
        public Vector3 Destination
        {
            get
            {
                return destination;
            }
            set
            {
                stopped = false;
                if (lastUpdateTime + delta < Time.time)
                {
                    destination = value;
                    lastUpdateTime = Time.time;
                    StopAllCoroutines();
                    GridScript.AddToQueue(this);
                }
            }
        }

        /// <summary>
        /// Задаёт путь
        /// </summary>
        /// <param name="path"></param>
        public void SetPath(List<Vector3> path)
        {
            this.path = path;
            if (path != null && path.Count != 0)
                StartCoroutine(FollowPath());
        }

        private void OnDrawGizmos()
        {
            if (path != null)
            {
                foreach (Vector3 p in path)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(p, Vector3.one / 2);
                }
            }
            Gizmos.color = Color.green;
            Gizmos.DrawCube(destination, Vector3.one / 2);
        }

        /// <summary>
        /// Останавливает агента
        /// </summary>
        public void Stop()
        {
            stopped = true;                               
        }
    }
}