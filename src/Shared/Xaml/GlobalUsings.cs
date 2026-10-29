// Shared/Xaml의 모든 파일이 공유하는 XAML 네임스페이스. 파일마다 #if using 블록을 반복하지 않도록 여기서 한 번만 고른다.
#if WINUI3 || UNO
global using Microsoft.UI.Xaml;
global using Microsoft.UI.Xaml.Controls;
global using Microsoft.UI.Xaml.Controls.Primitives;
global using Microsoft.UI.Xaml.Media.Animation;
#elif UWP
global using Windows.UI.Xaml;
global using Windows.UI.Xaml.Controls;
global using Windows.UI.Xaml.Controls.Primitives;
global using Windows.UI.Xaml.Media.Animation;
#else // WPF, OPENSILVER
global using System.Windows;
global using System.Windows.Controls;
global using System.Windows.Controls.Primitives;
global using System.Windows.Media.Animation;
#endif
