﻿using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MvvmValidation;
using VisualNovelManagerv2.Design;
using VisualNovelManagerv2.Design.VisualNovel;
using VisualNovelManagerv2.Infrastructure;
using VndbSharp;
using VndbSharp.Models;

// ReSharper disable ExplicitCallerInfoArgument

namespace VisualNovelManagerv2.ViewModel.VisualNovels
{
    public class AddVnViewModel: ValidatableViewModelBase
    {        
        private readonly AddVnViewModelService _service;
        public RelayCommand GetFile { get; private set; }
        public ICommand ValidateCommand { get; private set; }

        public AddVnViewModel()
        {
            //for openFileDialog
            this.GetFile = new RelayCommand(() => Messenger.Default.Send(_service));
            _service = new AddVnViewModelService {FilePicked = this.FilePicked};
            //for mvvmValidation
            ValidateCommand = new RelayCommand(Validate);
            ConfigureValidationRules();
            Validator.ResultChanged += OnValidationResultChanged;
        }

        #region Static Properties

        private bool? _isValid;
        public bool? IsValid
        {
            get { return _isValid; }
            private set
            {
                _isValid = value;
                RaisePropertyChanged(nameof(IsValid));
            }
        }

        private string _validationErrorsString;
        public string ValidationErrorsString
        {
            get { return _validationErrorsString; }
            private set
            {
                _validationErrorsString = value;
                RaisePropertyChanged(nameof(ValidationErrorsString));
            }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged(nameof(FileName));
                Validator.ValidateAsync(FileName);

            }
        }

        private uint _vnId;
        public uint VnId
        {
            get { return _vnId; }
            set
            {
                _vnId = value;
                RaisePropertyChanged(nameof(VnId));
                Validator.ValidateAsync(VnId);
            }
        }

        #endregion

        private void FilePicked()
        {
            this.FileName = _service.PickedFileName;
        }

        private void ConfigureValidationRules()
        {
            Validator.AddRequiredRule(()=> VnId, "Vndb ID is required");
            Validator.AddRule(nameof(VnId),
                () => RuleResult.Assert(VnId >= 1, "Vndb ID must be at leat 1"));
	        Validator.AddRule(nameof(VnId),
		        () => RuleResult.Assert(VnId <= GetDatabaseVnCount().Result, "Not a Valid Vndb ID"));
			Validator.AddRequiredRule(() => FileName, "Path to application is required");
            Validator.AddRule(nameof(FileName),
                () =>
                {
                    bool filepath = File.Exists(FileName);
                    string ext = Path.GetExtension(FileName) ?? string.Empty;
                    return RuleResult.Assert(filepath && ext.EndsWith(".exe"), "Not a valid file path");
                });
        }

        private static async Task<uint> GetDatabaseVnCount()
        {
	        using (Vndb client = new Vndb(true).WithClientDetails("VisualNovelManagerv2", "0.0.0"))
	        {
		        DatabaseStats stats = await client.GetDatabaseStatsAsync();
				client.Logout();
		        return stats.VisualNovels;
	        }
		}

        private void ValidateExe()
        {
            if (!File.Exists(FileName)) return;
            var twoBytes = new byte[2];
            using (FileStream fileStream = File.Open(FileName, FileMode.Open))
            {
                fileStream.Read(twoBytes, 0, 2);
            }
            switch (Encoding.UTF8.GetString(twoBytes))
            {
                case "MZ":
                    break;
                case "ZM":
                    break;
                default:
                    Validator.AddRule(nameof(FileName),
                        () => RuleResult.Invalid("This application is invalid"));
                    break;
            }
        }

        #region Validation Methods
        private async void Validate()
        {
            ValidateExe();
            await ValidateAsync();
        }

        private async Task ValidateAsync()
        {
            var result = await Validator.ValidateAllAsync();

            UpdateValidationSummary(result);
        }

        private void OnValidationResultChanged(object sender, ValidationResultChangedEventArgs e)
        {
            if (!IsValid.GetValueOrDefault(true))
            {
                ValidationResult validationResult = Validator.GetResult();

                UpdateValidationSummary(validationResult);
            }
        }

        private void UpdateValidationSummary(ValidationResult validationResult)
        {
            IsValid = validationResult.IsValid;
            ValidationErrorsString = validationResult.ToString();
        }
        #endregion
    }
}