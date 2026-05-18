using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OfficeSuite.Core.Services
{
    public class DocumentTemplateService
    {
        private readonly Dictionary<string, DocumentTemplate> _templates = new Dictionary<string, DocumentTemplate>();

        public DocumentTemplateService()
        {
            InitializeDefaultTemplates();
        }

        private void InitializeDefaultTemplates()
        {
            _templates["Blank"] = new DocumentTemplate
            {
                Id = "blank",
                Name = "Blank Document",
                Category = "General",
                Description = "A blank document with default settings",
                Thumbnail = null
            };

            _templates["Letter"] = new DocumentTemplate
            {
                Id = "letter",
                Name = "Letter",
                Category = "Letters",
                Description = "Professional letter template",
                Content = CreateLetterContent()
            };

            _templates["Memo"] = new DocumentTemplate
            {
                Id = "memo",
                Name = "Memo",
                Category = "Business",
                Description = "Internal memorandum template",
                Content = CreateMemoContent()
            };

            _templates["Report"] = new DocumentTemplate
            {
                Id = "report",
                Name = "Report",
                Category = "Business",
                Description = "Formal report template",
                Content = CreateReportContent()
            };

            _templates["Budget"] = new DocumentTemplate
            {
                Id = "budget",
                Name = "Budget Planner",
                Category = "Financial",
                Description = "Personal or business budget tracker",
                Content = CreateBudgetContent()
            };

            _templates["Invoice"] = new DocumentTemplate
            {
                Id = "invoice",
                Name = "Invoice",
                Category = "Financial",
                Description = "Professional invoice template",
                Content = CreateInvoiceContent()
            };

            _templates["Calendar"] = new DocumentTemplate
            {
                Id = "calendar",
                Name = "Calendar",
                Category = "Personal",
                Description = "Monthly calendar template",
                Content = CreateCalendarContent()
            };

            _templates["Agenda"] = new DocumentTemplate
            {
                Id = "agenda",
                Name = "Meeting Agenda",
                Category = "Business",
                Description = "Meeting agenda template",
                Content = CreateAgendaContent()
            };

            _templates["Timesheet"] = new DocumentTemplate
            {
                Id = "timesheet",
                Name = "Timesheet",
                Category = "Financial",
                Description = "Employee timesheet template",
                Content = CreateTimesheetContent()
            };

            _templates["ProjectPlan"] = new DocumentTemplate
            {
                Id = "projectplan",
                Name = "Project Plan",
                Category = "Business",
                Description = "Project planning template",
                Content = CreateProjectPlanContent()
            };
        }

        private string CreateLetterContent()
        {
            return @"[Date]

[Recipient Name]
[Company Name]
[Address Line 1]
[Address Line 2]
[City, State ZIP]

Dear [Recipient Name],

I am writing to you regarding [subject]. 

[Main body of letter]

[Closing statement]

Sincerely,

[Your Name]
[Your Title]";
        }

        private string CreateMemoContent()
        {
            return @"MEMORANDUM

TO: [Recipient]
FROM: [Sender]
DATE: [Date]
RE: [Subject]

[Main content]";
        }

        private string CreateReportContent()
        {
            return @"EXECUTIVE SUMMARY

[Summary of the report]

1. INTRODUCTION
   [Background and purpose]

2. FINDINGS
   [Main findings and analysis]

3. RECOMMENDATIONS
   [Suggested actions]

4. CONCLUSION
   [Final thoughts]

APPENDICES
[Supporting materials]";
        }

        private string CreateBudgetContent()
        {
            return @"MONTHLY BUDGET

Income:
- Salary: $_____
- Other: $_____
- Total Income: $_____

Fixed Expenses:
- Rent/Mortgage: $_____
- Utilities: $_____
- Insurance: $_____
- Total Fixed: $_____

Variable Expenses:
- Groceries: $_____
- Transportation: $_____
- Entertainment: $_____
- Other: $_____
- Total Variable: $_____

SAVINGS: $_____";
        }

        private string CreateInvoiceContent()
        {
            return @"INVOICE

Invoice #: _____
Date: _____
Due Date: _____

Bill To:
[Client Name]
[Address]

From:
[Your Company]
[Your Address]

Description          Quantity    Unit Price    Amount
------------------------------------------------------
[Item 1]             [Qty]       $[Price]      $[Total]
[Item 2]             [Qty]       $[Price]      $[Total]

                                    Subtotal:    $_____
                                    Tax:         $_____
                                    TOTAL:       $_____";
        }

        private string CreateCalendarContent()
        {
            return @"CALENDAR - [Month Year]

Sun    Mon    Tue    Wed    Thu    Fri    Sat
-------------------------------------------------
[Week 1 dates]
[Week 2 dates]
[Week 3 dates]
[Week 4 dates]
[Week 5 dates]";
        }

        private string CreateAgendaContent()
        {
            return @"MEETING AGENDA

Date: _____
Time: _____
Location: _____

Attendees:
- [Name 1]
- [Name 2]

1. Call to Order [Time]
2. Approval of Previous Minutes [Time]
3. Old Business [Time]
4. New Business [Time]
   - [Topic A]
   - [Topic B]
5. Announcements [Time]
6. Adjournment [Time]

Next Meeting: [Date, Time, Location]";
        }

        private string CreateTimesheetContent()
        {
            return @"TIMESHEET

Employee: _____
Department: _____
Period: _____

Date       | Project    | Hours | Description
-------------------------------------------------
[Date 1]   | [Project]  | [Hrs] | [Desc]
[Date 2]   | [Project]  | [Hrs] | [Desc]
[Date 3]   | [Project]  | [Hrs] | [Desc]

Total Hours: _____

Employee Signature: _____ Date: _____
Supervisor Signature: _____ Date: _____";
        }

        private string CreateProjectPlanContent()
        {
            return @"PROJECT PLAN

Project Name: _____
Project Manager: _____
Start Date: _____
End Date: _____

OBJECTIVES:
1. [Objective 1]
2. [Objective 2]
3. [Objective 3]

MILESTONES:
- Milestone 1: [Date]
- Milestone 2: [Date]
- Milestone 3: [Date]

TASKS:
Task Name          | Owner    | Start    | End      | Status
--------------------------------------------------------------
[Task 1]           | [Name]   | [Date]   | [Date]   | [Status]
[Task 2]           | [Name]   | [Date]   | [Date]   | [Status]

RESOURCES:
- [Resource 1]
- [Resource 2]

RISKS:
- [Risk 1]
- [Risk 2]";
        }

        public IEnumerable<DocumentTemplate> GetTemplates(string category = null)
        {
            if (string.IsNullOrEmpty(category))
                return _templates.Values;

            return _templates.Values.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetCategories()
        {
            return _templates.Values.Select(t => t.Category).Distinct().OrderBy(c => c);
        }

        public DocumentTemplate GetTemplate(string id)
        {
            return _templates.TryGetValue(id, out var template) ? template : null;
        }

        public void AddTemplate(DocumentTemplate template)
        {
            if (template != null && !string.IsNullOrEmpty(template.Id))
                _templates[template.Id] = template;
        }

        public bool RemoveTemplate(string id)
        {
            return _templates.Remove(id);
        }

        public DocumentTemplate CreateFromTemplate(string templateId, Dictionary<string, string> placeholders = null)
        {
            var template = GetTemplate(templateId);
            if (template == null) return null;

            var newTemplate = new DocumentTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Name = template.Name,
                Category = template.Category,
                Description = template.Description,
                Content = template.Content
            };

            if (placeholders != null && !string.IsNullOrEmpty(newTemplate.Content))
            {
                foreach (var placeholder in placeholders)
                {
                    newTemplate.Content = newTemplate.Content.Replace($"[{placeholder.Key}]", placeholder.Value);
                }
            }

            return newTemplate;
        }
    }

    public class DocumentTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public byte[] Thumbnail { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class ValidationService
    {
        private readonly List<ValidationRule> _rules = new List<ValidationRule>();

        public ValidationService()
        {
            InitializeDefaultRules();
        }

        private void InitializeDefaultRules()
        {
            _rules.Add(new ValidationRule
            {
                Name = "Required",
                ErrorMessage = "This field is required",
                Validate = value => !string.IsNullOrWhiteSpace(value?.ToString())
            });

            _rules.Add(new ValidationRule
            {
                Name = "Email",
                ErrorMessage = "Invalid email format",
                Validate = value => IsValidEmail(value?.ToString())
            });

            _rules.Add(new ValidationRule
            {
                Name = "Phone",
                ErrorMessage = "Invalid phone number format",
                Validate = value => IsValidPhone(value?.ToString())
            });

            _rules.Add(new ValidationRule
            {
                Name = "URL",
                ErrorMessage = "Invalid URL format",
                Validate = value => IsValidUrl(value?.ToString())
            });

            _rules.Add(new ValidationRule
            {
                Name = "Numeric",
                ErrorMessage = "Value must be numeric",
                Validate = value => IsNumeric(value)
            });

            _rules.Add(new ValidationRule
            {
                Name = "PositiveNumber",
                ErrorMessage = "Value must be a positive number",
                Validate = value => IsPositiveNumber(value)
            });

            _rules.Add(new ValidationRule
            {
                Name = "Date",
                ErrorMessage = "Invalid date format",
                Validate = value => IsValidDate(value)
            });

            _rules.Add(new ValidationRule
            {
                Name = "MaxLength",
                ErrorMessage = "Value exceeds maximum length",
                Validate = (value, param) => value?.ToString().Length <= int.Parse(param)
            });

            _rules.Add(new ValidationRule
            {
                Name = "MinLength",
                ErrorMessage = "Value is below minimum length",
                Validate = (value, param) => value?.ToString().Length >= int.Parse(param)
            });

            _rules.Add(new ValidationRule
            {
                Name = "Range",
                ErrorMessage = "Value is outside the allowed range",
                Validate = (value, param) =>
                {
                    if (!IsNumeric(value)) return false;
                    var parts = param.Split(',');
                    if (parts.Length != 2) return false;
                    var num = double.Parse(value.ToString());
                    return num >= double.Parse(parts[0]) && num <= double.Parse(parts[1]);
                }
            });

            _rules.Add(new ValidationRule
            {
                Name = "Pattern",
                ErrorMessage = "Value does not match the required pattern",
                Validate = (value, param) => !string.IsNullOrEmpty(value?.ToString()) && Regex.IsMatch(value.ToString(), param)
            });

            _rules.Add(new ValidationRule
            {
                Name = "CreditCard",
                ErrorMessage = "Invalid credit card number",
                Validate = value => IsValidCreditCard(value?.ToString())
            });

            _rules.Add(new ValidationRule
            {
                Name = "SSN",
                ErrorMessage = "Invalid Social Security Number",
                Validate = value => IsValidSSN(value?.ToString())
            });

            _rules.Add(new ValidationRule
            {
                Name = "ZipCode",
                ErrorMessage = "Invalid ZIP code format",
                Validate = value => IsValidZipCode(value?.ToString())
            });
        }

        public ValidationResult Validate(object value, string ruleName, string parameter = null)
        {
            var rule = _rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
            if (rule == null)
                return new ValidationResult { IsValid = false, ErrorMessage = $"Unknown validation rule: {ruleName}" };

            bool isValid = rule.Validate(value, parameter);
            return new ValidationResult
            {
                IsValid = isValid,
                ErrorMessage = isValid ? null : rule.ErrorMessage
            };
        }

        public IEnumerable<ValidationResult> ValidateMultiple(object value, IEnumerable<string> ruleNames)
        {
            foreach (var ruleName in ruleNames)
            {
                yield return Validate(value, ruleName);
            }
        }

        private bool IsValidEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(value);
            }
            catch { return false; }
        }

        private bool IsValidPhone(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            try
            {
                var regex = new Regex(@"^\+?[\d\s\-\(\)]{10,}$");
                return regex.IsMatch(value);
            }
            catch { return false; }
        }

        private bool IsValidUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return Uri.TryCreate(value, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private bool IsNumeric(object value)
        {
            if (value == null) return false;
            return double.TryParse(value.ToString(), out _);
        }

        private bool IsPositiveNumber(object value)
        {
            if (value == null) return false;
            if (double.TryParse(value.ToString(), out double num))
                return num > 0;
            return false;
        }

        private bool IsValidDate(object value)
        {
            if (value == null) return false;
            return DateTime.TryParse(value.ToString(), out _);
        }

        private bool IsValidCreditCard(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Replace("-", "").Replace(" ", "");
            if (value.Length < 13 || value.Length > 19) return false;
            if (!long.TryParse(value, out _)) return false;

            int sum = 0;
            bool alternate = false;
            for (int i = value.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(value[i].ToString());
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }
            return (sum % 10 == 0);
        }

        private bool IsValidSSN(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var regex = new Regex(@"^\d{3}-\d{2}-\d{4}$");
            return regex.IsMatch(value);
        }

        private bool IsValidZipCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var regex = new Regex(@"^\d{5}(-\d{4})?$");
            return regex.IsMatch(value);
        }
    }

    public class ValidationRule
    {
        public string Name { get; set; }
        public string ErrorMessage { get; set; }
        public Func<object, bool> Validate { get; private set; }
        public Func<object, string, bool> ValidateWithParam { get; private set; }

        public ValidationRule()
        {
            Validate = value => true;
            ValidateWithParam = (value, param) => true;
        }

        public ValidationRule(Func<object, bool> validate)
        {
            Validate = validate;
            ValidateWithParam = (value, param) => validate(value);
        }

        public ValidationRule(Func<object, string, bool> validate)
        {
            ValidateWithParam = validate;
            Validate = value => validate(value, null);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string PropertyName { get; set; }
    }

    public class EncryptionService
    {
        public string Encrypt(string plainText, string key = null)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = new byte[plainBytes.Length];

                for (int i = 0; i < plainBytes.Length; i++)
                {
                    encryptedBytes[i] = (byte)(plainBytes[i] ^ (key?[i % key.Length] ?? 0x42));
                }

                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return plainText;
            }
        }

        public string Decrypt(string encryptedText, string key = null)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = new byte[encryptedBytes.Length];

                for (int i = 0; i < encryptedBytes.Length; i++)
                {
                    plainBytes[i] = (byte)(encryptedBytes[i] ^ (key?[i % key.Length] ?? 0x42));
                }

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return encryptedText;
            }
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            return HashPassword(password) == hashedPassword;
        }

        public string GenerateSalt(int length = 32)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            byte[] saltBytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                saltBytes[i] = (byte)chars[random.Next(chars.Length)];
            }
            return Convert.ToBase64String(saltBytes);
        }

        public string HashWithSalt(string password, string salt)
        {
            var combined = password + salt;
            return HashPassword(combined);
        }
    }

    public class SettingsService
    {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        public T Get<T>(string key, T defaultValue = default)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;

                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public void Set<T>(string key, T value)
        {
            if (value != null)
                _settings[key] = value;
            else
                _settings.Remove(key);
        }

        public bool Contains(string key)
        {
            return _settings.ContainsKey(key);
        }

        public void Remove(string key)
        {
            _settings.Remove(key);
        }

        public void Clear()
        {
            _settings.Clear();
        }

        public IDictionary<string, object> GetAll()
        {
            return new Dictionary<string, object>(_settings);
        }

        public void LoadFromDictionary(IDictionary<string, object> dictionary)
        {
            Clear();
            foreach (var kvp in dictionary)
            {
                _settings[kvp.Key] = kvp.Value;
            }
        }

        public string Serialize()
        {
            var sb = new StringBuilder();
            foreach (var kvp in _settings)
            {
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
            }
            return sb.ToString();
        }

        public void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            Clear();
            var lines = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    _settings[parts[0]] = parts[1];
                }
            }
        }
    }

    public class LocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>();
        private string _currentLanguage = "en-US";

        public LocalizationService()
        {
            InitializeDefaultTranslations();
        }

        private void InitializeDefaultTranslations()
        {
            _translations["en-US"] = new Dictionary<string, string>
            {
                ["File"] = "File",
                ["Edit"] = "Edit",
                ["View"] = "View",
                ["Insert"] = "Insert",
                ["Format"] = "Format",
                ["Tools"] = "Tools",
                ["Table"] = "Table",
                ["Help"] = "Help",
                ["New"] = "New",
                ["Open"] = "Open",
                ["Save"] = "Save",
                ["SaveAs"] = "Save As",
                ["Close"] = "Close",
                ["Exit"] = "Exit",
                ["Undo"] = "Undo",
                ["Redo"] = "Redo",
                ["Cut"] = "Cut",
                ["Copy"] = "Copy",
                ["Paste"] = "Paste",
                ["Delete"] = "Delete",
                ["SelectAll"] = "Select All",
                ["Find"] = "Find",
                ["Replace"] = "Replace",
                ["Print"] = "Print",
                ["Properties"] = "Properties",
                ["About"] = "About",
                ["OK"] = "OK",
                ["Cancel"] = "Cancel",
                ["Yes"] = "Yes",
                ["No"] = "No",
                ["Error"] = "Error",
                ["Warning"] = "Warning",
                ["Information"] = "Information",
                ["Confirm"] = "Confirm"
            };

            _translations["es-ES"] = new Dictionary<string, string>
            {
                ["File"] = "Archivo",
                ["Edit"] = "Editar",
                ["View"] = "Ver",
                ["Insert"] = "Insertar",
                ["Format"] = "Formato",
                ["Tools"] = "Herramientas",
                ["Table"] = "Tabla",
                ["Help"] = "Ayuda",
                ["New"] = "Nuevo",
                ["Open"] = "Abrir",
                ["Save"] = "Guardar",
                ["SaveAs"] = "Guardar Como",
                ["Close"] = "Cerrar",
                ["Exit"] = "Salir",
                ["Undo"] = "Deshacer",
                ["Redo"] = "Rehacer",
                ["Cut"] = "Cortar",
                ["Copy"] = "Copiar",
                ["Paste"] = "Pegar",
                ["Delete"] = "Eliminar",
                ["SelectAll"] = "Seleccionar Todo",
                ["Find"] = "Buscar",
                ["Replace"] = "Reemplazar",
                ["Print"] = "Imprimir",
                ["Properties"] = "Propiedades",
                ["About"] = "Acerca de",
                ["OK"] = "Aceptar",
                ["Cancel"] = "Cancelar",
                ["Yes"] = "Sí",
                ["No"] = "No",
                ["Error"] = "Error",
                ["Warning"] = "Advertencia",
                ["Information"] = "Información",
                ["Confirm"] = "Confirmar"
            };

            _translations["fr-FR"] = new Dictionary<string, string>
            {
                ["File"] = "Fichier",
                ["Edit"] = "Édition",
                ["View"] = "Affichage",
                ["Insert"] = "Insertion",
                ["Format"] = "Format",
                ["Tools"] = "Outils",
                ["Table"] = "Tableau",
                ["Help"] = "Aide",
                ["New"] = "Nouveau",
                ["Open"] = "Ouvrir",
                ["Save"] = "Enregistrer",
                ["SaveAs"] = "Enregistrer sous",
                ["Close"] = "Fermer",
                ["Exit"] = "Quitter",
                ["Undo"] = "Annuler",
                ["Redo"] = "Rétablir",
                ["Cut"] = "Couper",
                ["Copy"] = "Copier",
                ["Paste"] = "Coller",
                ["Delete"] = "Supprimer",
                ["SelectAll"] = "Tout sélectionner",
                ["Find"] = "Rechercher",
                ["Replace"] = "Remplacer",
                ["Print"] = "Imprimer",
                ["Properties"] = "Propriétés",
                ["About"] = "À propos",
                ["OK"] = "OK",
                ["Cancel"] = "Annuler",
                ["Yes"] = "Oui",
                ["No"] = "Non",
                ["Error"] = "Erreur",
                ["Warning"] = "Avertissement",
                ["Information"] = "Information",
                ["Confirm"] = "Confirmer"
            };

            _translations["de-DE"] = new Dictionary<string, string>
            {
                ["File"] = "Datei",
                ["Edit"] = "Bearbeiten",
                ["View"] = "Ansicht",
                ["Insert"] = "Einfügen",
                ["Format"] = "Format",
                ["Tools"] = "Extras",
                ["Table"] = "Tabelle",
                ["Help"] = "Hilfe",
                ["New"] = "Neu",
                ["Open"] = "Öffnen",
                ["Save"] = "Speichern",
                ["SaveAs"] = "Speichern unter",
                ["Close"] = "Schließen",
                ["Exit"] = "Beenden",
                ["Undo"] = "Rückgängig",
                ["Redo"] = "Wiederholen",
                ["Cut"] = "Ausschneiden",
                ["Copy"] = "Kopieren",
                ["Paste"] = "Einfügen",
                ["Delete"] = "Löschen",
                ["SelectAll"] = "Alles auswählen",
                ["Find"] = "Suchen",
                ["Replace"] = "Ersetzen",
                ["Print"] = "Drucken",
                ["Properties"] = "Eigenschaften",
                ["About"] = "Über",
                ["OK"] = "OK",
                ["Cancel"] = "Abbrechen",
                ["Yes"] = "Ja",
                ["No"] = "Nein",
                ["Error"] = "Fehler",
                ["Warning"] = "Warnung",
                ["Information"] = "Information",
                ["Confirm"] = "Bestätigen"
            };
        }

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_translations.ContainsKey(value))
                    _currentLanguage = value;
            }
        }

        public IEnumerable<string> AvailableLanguages => _translations.Keys;

        public string Translate(string key)
        {
            if (_translations.TryGetValue(_currentLanguage, out var languageDictionary))
            {
                if (languageDictionary.TryGetValue(key, out var translation))
                    return translation;
            }

            if (_translations.TryGetValue("en-US", out var defaultDictionary))
            {
                if (defaultDictionary.TryGetValue(key, out var defaultTranslation))
                    return defaultTranslation;
            }

            return key;
        }

        public string Translate(string key, params object[] args)
        {
            var translation = Translate(key);
            return string.Format(translation, args);
        }

        public void AddTranslation(string language, string key, string value)
        {
            if (!_translations.ContainsKey(language))
                _translations[language] = new Dictionary<string, string>();

            _translations[language][key] = value;
        }

        public void AddLanguage(string language)
        {
            if (!_translations.ContainsKey(language))
                _translations[language] = new Dictionary<string, string>();
        }
    }

    public class PluginService
    {
        private readonly List<OfficePlugin> _plugins = new List<OfficePlugin>();
        private readonly Dictionary<string, Type> _registeredCommands = new Dictionary<string, Type>();

        public event EventHandler<PluginEventArgs> PluginLoaded;
        public event EventHandler<PluginEventArgs> PluginUnloaded;

        public IEnumerable<OfficePlugin> Plugins => _plugins.AsReadOnly();

        public void LoadPlugin(OfficePlugin plugin)
        {
            if (plugin == null) return;

            plugin.Initialize();
            _plugins.Add(plugin);
            PluginLoaded?.Invoke(this, new PluginEventArgs { Plugin = plugin });
        }

        public void UnloadPlugin(string pluginId)
        {
            var plugin = _plugins.FirstOrDefault(p => p.Id == pluginId);
            if (plugin != null)
            {
                plugin.Cleanup();
                _plugins.Remove(plugin);
                PluginUnloaded?.Invoke(this, new PluginEventArgs { Plugin = plugin });
            }
        }

        public OfficePlugin GetPlugin(string pluginId)
        {
            return _plugins.FirstOrDefault(p => p.Id == pluginId);
        }

        public void RegisterCommand(string commandId, Type commandType)
        {
            _registeredCommands[commandId] = commandType;
        }

        public Type GetCommandType(string commandId)
        {
            return _registeredCommands.TryGetValue(commandId, out var type) ? type : null;
        }

        public IEnumerable<T> GetPluginsByType<T>() where T : OfficePlugin
        {
            return _plugins.OfType<T>();
        }

        public void ExecuteCommand(string commandId, object parameter = null)
        {
            var commandType = GetCommandType(commandId);
            if (commandType != null)
            {
                var command = Activator.CreateInstance(commandType) as IOfficeCommand;
                command?.Execute(parameter);
            }
        }
    }

    public class OfficePlugin
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public byte[] Icon { get; set; }

        public virtual void Initialize() { }
        public virtual void Cleanup() { }
        public virtual void OnDocumentOpened(OfficeDocument document) { }
        public virtual void OnDocumentSaved(OfficeDocument document) { }
        public virtual void OnDocumentClosed(OfficeDocument document) { }
    }

    public class PluginEventArgs : EventArgs
    {
        public OfficePlugin Plugin { get; set; }
    }

    public interface IOfficeCommand
    {
        void Execute(object parameter);
    }

    public class OfficeDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public byte[] Content { get; set; }
        public DateTime LastModified { get; set; }
    }

    public enum DocumentType
    {
        Word,
        Spreadsheet,
        Presentation
    }
}
