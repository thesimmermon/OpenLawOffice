﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenLawOffice.WinClient
{
    public class ControllerManager : Common.Singleton<ControllerManager>
    {
        // Common.Model.X, Controller
        private Dictionary<Type, Controllers.ControllerBase> _modelMapToController;

        // Controller, Controller
        private Dictionary<Type, Controllers.ControllerBase> _controllers;

        public ControllerManager()
        {
            _modelMapToController = new Dictionary<Type, Controllers.ControllerBase>();
            _controllers = new Dictionary<Type, Controllers.ControllerBase>();
        }

        public void ScanAssembly(System.Reflection.Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in assembly.GetTypes())
            {
                ScanType(type);
            }
        }

        public void ScanType(Type type)
        {
            foreach (Controllers.HandleAttribute att in type.GetCustomAttributes(typeof(Controllers.HandleAttribute), false))
            {
                if (!_controllers.ContainsKey(type))
                {
                    _controllers.Add(type, (Controllers.ControllerBase)type.GetConstructor(new Type[] { }).Invoke(null));
                }

                _modelMapToController.Add(att.ModelType, _controllers[type]);
            }
        }

        public T GetController<T>()
            where T : Controllers.ControllerBase
        {
            return (T)_controllers[typeof(T)];
        }

        public void LoadUI<TModel>(ViewModels.IViewModel selected)
            where TModel : Common.Models.ModelBase
        {
            if (!_modelMapToController.ContainsKey(typeof(TModel)))
                throw new ArgumentException("Type argument cannot be found in Model mappings.");
            
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                _modelMapToController[typeof(TModel)].LoadUI(selected);
            }));
        }

        public void LoadUI<TModel>()
            where TModel : Common.Models.ModelBase
        {
            LoadUI<TModel>(null);
        }

        public void LoadItems<TModel>(ViewModels.IViewModel filter, ICollection<ViewModels.IViewModel> collection, Action<ICollection<ViewModels.IViewModel>, ErrorHandling.ActionableError> onComplete)
            where TModel : Common.Models.ModelBase
        {
            _modelMapToController[typeof(TModel)].LoadItems(filter, collection, onComplete);
        }

        public void LoadItems(ViewModels.IViewModel filter, ICollection<ViewModels.IViewModel> collection, Action<ICollection<ViewModels.IViewModel>, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[LookupModelType(filter)].LoadItems(filter, collection, onComplete);
        }

        public void LoadDetails<TModel>(ViewModels.IViewModel viewModel, Action<ViewModels.IViewModel, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[typeof(TModel)].LoadDetails(viewModel, onComplete);
        }

        public void LoadDetails(ViewModels.IViewModel viewModel, Action<ViewModels.IViewModel, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[LookupModelType(viewModel)].LoadDetails(viewModel, onComplete);
        }

        public void UpdateItem<TModel>(ViewModels.IViewModel viewModel, Action<ViewModels.IViewModel, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[typeof(TModel)].UpdateItem(viewModel, onComplete);
        }

        public void UpdateItem(ViewModels.IViewModel viewModel, Action<ViewModels.IViewModel, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[LookupModelType(viewModel)].UpdateItem(viewModel, onComplete);
        }

        public void CreateItem<TModel>(ViewModels.IViewModel viewModel, Action<ViewModels.IViewModel, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[typeof(TModel)].CreateItem(viewModel, onComplete);
        }

        public void CreateItem(ViewModels.IViewModel viewModel, Action<ViewModels.IViewModel, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[LookupModelType(viewModel)].CreateItem(viewModel, onComplete);
        }

        public void DisableItem<TModel>(ViewModels.IViewModel viewModel, Action<ViewModels.IViewModel, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[typeof(TModel)].DisableItem(viewModel, onComplete);
        }

        public void DisableItem(ViewModels.IViewModel viewModel, Action<ViewModels.IViewModel, ErrorHandling.ActionableError> onComplete)
        {
            _modelMapToController[LookupModelType(viewModel)].DisableItem(viewModel, onComplete);
        }

        public void SelectItem<TModel>(ViewModels.IViewModel viewModel)
            where TModel : Common.Models.ModelBase
        {
            if (!_modelMapToController.ContainsKey(typeof(TModel)))
                throw new ArgumentException("Type argument cannot be found in Model mappings.");

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                _modelMapToController[typeof(TModel)].SelectItem(viewModel);
            }));
        }

        public void SetDisplayMode<TModel>(Controls.DisplayModeType mode)
            where TModel : Common.Models.ModelBase
        {
            if (!_modelMapToController.ContainsKey(typeof(TModel)))
                throw new ArgumentException("Type argument cannot be found in Model mappings.");

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                _modelMapToController[typeof(TModel)].SetDisplayMode(mode);
            }));
        }

        private Type LookupModelType(ViewModels.IViewModel viewModel)
        {
            System.Reflection.FieldInfo fi = viewModel.GetType().GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (fi == null)
                throw new ArgumentException("Object does not have a _model field.");

            Common.Models.ModelBase model = (Common.Models.ModelBase)fi.GetValue(viewModel);

            return model.GetType();
        }
    }
}
