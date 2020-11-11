# Localization
A library for .net that can help you to manage the localization in your application with a simple method.

# Architecture
line...

# LocalizationManager
line...
```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { });
```

```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { }, new CultureInfo("en-US"));
```

```c#
LocalizationManager.SetCulture(new CultureInfo("en-US"));
```

```c#
var label = LocalizationManager.Instance["resourceKey"];
```

```c#
var label = LocalizationManager.Translate("resourceKey","#Default value");
```

# Vocabolary Service Provider
line...
```c# xaml
public class MockVocabolaryServiceProvider : IVocabolaryServiceProvider
{
    Dictionary<string, Vocabolary> all = null;

    public Task AddOrUpdateTermAsync(IVocabolary vocabolary, string key, string defaultValue = null) 
        => Task.Delay(1);

    public Task Initialize()
    {
        all = new Dictionary<string, Vocabolary>
        {
            {"it-IT", new Vocabolary {
                    { "MainWindow","Main Window in italian language" }
                }
            },
            {"en-US", new Vocabolary {
                    { "MainWindow","Main Window in english language" } 
                } 
            }
        };
        return Task.FromResult(all);
    }

    public Task<IVocabolary> LoadVocabolaryAsync(CultureInfo cultureInfo)
    {
        string cultureDefault = "it-IT";
        if (all.ContainsKey(cultureInfo.ToString()))
            cultureDefault = cultureInfo.ToString();
        return Task.FromResult<IVocabolary>(all[cultureDefault]);
    }

    public Task SaveAsync(IVocabolary vocabolary) 
        => Task.Delay(1);
}
```

# Localization Uses
line...

# Xamarin
line...
```c# xaml
 xmlns:culture="clr-namespace:Localization.Xamarin;assembly=Localization.Xamarin"
```

```c# xaml
<Label Text="{culture:Translate MainWindow, DefaultValue=Main Window}"></Label>
```

# WPF
line...
```c# xaml
xmlns:culture="clr-namespace:Localization.Windows;assembly=Localization.Windows"
```

```c# xaml
<Label Content="{culture:Translate MainWindow, DefaultValue=Main Window}"></Label>
```
