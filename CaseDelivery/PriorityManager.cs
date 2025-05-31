using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CaseDelivery
{
    public class PriorityManager
    {
        public double CurrentPriority => _currentPriority;
        public string Description => _description;
        public Brush Color => _color;

        private readonly MainWindow _window;
        private double _currentPriority;
        private string _description;
        private Brush _color;


        public PriorityManager(MainWindow window)
        {
            _window = window;
            UpdatePriorityInfo(0);
        }

        public void SetPriority(double value)//обновление приоритета
        {
            if (_currentPriority != value)
            {
                _currentPriority = value;
                UpdatePriorityInfo(value);
                _window.OnPropertyChanged(nameof(_window.OrderPriority));
                _window.OnPropertyChanged(nameof(_window.PriorityDescription));
                _window.OnPropertyChanged(nameof(_window.PriorityColor));
            }
        }

        private void UpdatePriorityInfo(double priority)//зная значение изменяем цвет
        {
            if (priority <= 0.3)
            {
                _description = "Низкий приоритет";
                _color = Brushes.LimeGreen;
            }
            else if (priority <= 0.7)
            {
                _description = "Средний приоритет";
                _color = Brushes.Gold;
            }
            else if (priority <= 0.9)
            {
                _description = "Высокий приоритет";
                _color = Brushes.OrangeRed;
            }
            else
            {
                _description = "Наивысший приоритет";
                _color = Brushes.Red;
            }
        }
    }
}
