using System;
using UnityEngine;

class DayOfWeekVisibility : MonoBehaviour
{
    [SerializeField] private DayOfWeek _dayOfWeek;
    [SerializeField] private bool _destroyOnWrongDay = false;
    private void Awake()
    {
        if (_dayOfWeek == GameManager.CurrentDayOfWeek)
            return;

        if (_destroyOnWrongDay)
            DestroyImmediate(gameObject);
        else
            gameObject.SetActive(_dayOfWeek == GameManager.CurrentDayOfWeek);
    }
}