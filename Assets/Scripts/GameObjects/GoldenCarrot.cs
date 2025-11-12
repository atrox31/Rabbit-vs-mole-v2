using Extensions;
using System;
using UnityEngine;

namespace GameObjects
{
    public class GoldenCarrot : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Speed of rotation around the Y-axis in degrees per second.")]
        private float _rotationSpeed = 100f;

        [Header("Floating Movement Settings")]
        [Tooltip("Maximum distance the object will move up and down from its center point (in Unity units).")]
        private float _amplitude = 0.1f; // How far the object moves up/down

        [Tooltip("Speed of the up and down movement (frequency of the sine wave). A higher value means faster floating.")]
        private float _frequency = 2f; // How fast the floating cycles

        [SerializeField] private DayOfWeek _dayOfWeek;

        private Vector3 startPosition;
        [SerializeField] private Transform _model;
        [SerializeField] private ParticleSystem _collectParticles;
        [SerializeField] private ParticleSystem _shinyParticles;
        private void Awake()
        {
            if(_model == null)
            {
                Debug.LogError("Golden carrot ca not find model in children");
                Destroy(gameObject);
            }
            startPosition = _model.position;
            /* TODO: enable on tests
            bool carrotIsColected = false;
            gameObject.SetActive(
                !carrotIsColected && _dayOfWeek == DayOfWeekVisibility.currentDayOfWeek
                );
            */
        }

        private void Update()
        {
            _model.Rotate(Vector3.up * _rotationSpeed * Time.deltaTime, Space.World);

            float yOffset = Mathf.Sin(Time.time * _frequency) * _amplitude;
            float newY = startPosition.y + yOffset;

            _model.position = new Vector3(startPosition.x, newY, startPosition.z);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Collect();
            Destroy(gameObject);
        }

        public void Collect()
        {
            _collectParticles.DetachAndPlay();
            GameManager.GoldenCarrotPick();
        }
    }
}
