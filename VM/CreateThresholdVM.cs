using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace NDT_RevitAPI
{
    public class CreateThresholdVM : INotifyPropertyChanged
    {
        #region implement interface
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertychanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        private bool _isCreateSeparateFloors;
        public bool IsCreateSeparateFloors
        {
            get => _isCreateSeparateFloors;
            set
            {
                _isCreateSeparateFloors = value;
                OnPropertychanged();
            }
        }

        private double _heightOffset;
        public double HeightOffset
        {
            get => _heightOffset;
            set
            {
                _heightOffset = value;
                OnPropertychanged();
            }
        }

        private FloorTypeModel _selectedFloorType;
        public FloorTypeModel SelectedFloorType
        {
            get => _selectedFloorType;
            set
            {
                _selectedFloorType = value;
                OnPropertychanged();
            }
        }

        private ObservableCollection<FloorTypeModel> _floorTypes;
        public ObservableCollection<FloorTypeModel> FloorTypes
        {
            get => _floorTypes;
            set
            {
                _floorTypes = value;
                OnPropertychanged();
            }
        }
        #endregion

        #region Command
        public ICommand Cancel { get; }
        public ICommand OKCmd { get; }
        #endregion

        public CreateThresholdsView mainWindow;
        public CreateThresholdVM(ObservableCollection<FloorTypeModel> floorTypes, FloorTypeModel selectedFloorType,
            double heightOffset, bool isCreateSeparateFloors)
        {

            FloorTypes = floorTypes;
            SelectedFloorType = selectedFloorType;
            HeightOffset = heightOffset;
            IsCreateSeparateFloors = isCreateSeparateFloors;


            Cancel = new RelayCommand<Window>((p) => { mainWindow.Close(); });
            OKCmd = new RelayCommand<Window>((p) => { mainWindow.DialogResult = true; });
        }
        

    }
}
