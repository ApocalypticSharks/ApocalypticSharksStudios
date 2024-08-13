using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSelection : MonoBehaviour
{
    [SerializeField] private List<GameObject> _cars = new List<GameObject>();
    [SerializeField] private GameObject _lockedImage, _buyButton, _selectButton;
    [SerializeField] private Text _carPrice;
    private int _selectedCarIndex;
    private Dictionary<int, int> _carPrices = new Dictionary<int, int> 
    {
        [0] = 0,
        [1] = 500,
        [2] = 1000,
        [3] = 2000,
        [4] = 3000,
        [5] = 15000
    };

    private void Awake()
    {
        _selectedCarIndex = 0;
        _cars[_selectedCarIndex].SetActive(true);
    }
    private void Update()
    {
        _carPrice.text = $"${_carPrices[_selectedCarIndex]}";
    }

    public void ToNextCar()
    {
        if (_selectedCarIndex < _cars.Count - 1)
        {
            _cars[_selectedCarIndex].SetActive(false);
            _selectedCarIndex++;
            _cars[_selectedCarIndex].SetActive(true);
            if (Progress.instance.savedGameData.unlockedCars.Contains(_selectedCarIndex))
            {
                _lockedImage.SetActive(false);
                _buyButton.SetActive(false);
                _selectButton.SetActive(true);
            }
            else
            {
                _lockedImage.SetActive(true);
                _buyButton.SetActive(true);
                _selectButton.SetActive(false);
            }
        }
    }
    public void ToPreviousCar()
    {
        if (_selectedCarIndex > 0)
        {
            _cars[_selectedCarIndex].SetActive(false);
            _selectedCarIndex--;
            _cars[_selectedCarIndex].SetActive(true);
            if (Progress.instance.savedGameData.unlockedCars.Contains(_selectedCarIndex))
            {
                _lockedImage.SetActive(false);
                _buyButton.SetActive(false);
                _selectButton.SetActive(true);
            }
            else
            {
                _lockedImage.SetActive(true);
                _buyButton.SetActive(true);
                _selectButton.SetActive(false);
            }
        }
    }

    public void BuyCar() 
    {
        if (Progress.instance.savedGameData.coins >= _carPrices[_selectedCarIndex])
        {
            Progress.instance.savedGameData.coins -= _carPrices[_selectedCarIndex];
            UIDataScript.instance.UpdateUIData();
            Progress.instance.savedGameData.unlockedCars.Add(_selectedCarIndex);
            _lockedImage.SetActive(false);
            _buyButton.SetActive(false);
            _selectButton.SetActive(true);
            Progress.instance.SaveData();
        }
    }

    public void SelectCar()
    {
        DataToKeep.selectedCar = _selectedCarIndex;
        SceneManager.LoadScene(1);
    }
}
