// XAML에서도 C#처럼 어셈블리명을 적지 않고 쓰게 하기 위한 설정.
//
// WPF와 OpenSilver의 XAML은 clr-namespace 문법이라 어셈블리명을 함께 적어야 한다.
// 그대로 두면 소비자가 assembly=Jamesnet.Foundation을 매번 쓰게 되므로,
// XmlnsDefinition으로 URI 하나에 묶는다.
//
//     xmlns:james="http://jamesnet.dev/xaml"
//     <james:XamlLayer LayerName="Main" />
//
// Uno / WinUI3 / UWP의 XAML은 using: 문법이라 어셈블리명이 애초에 필요 없다.
// 이쪽은 아래처럼 쓰면 된다. XmlnsDefinitionAttribute는 이 플랫폼들에서
// internal이라 소비자가 붙일 수 없고, XAML 컴파일러도 이를 참조하지 않는다.
//
//     xmlns:james="using:Jamesnet.Foundation"
//     <james:XamlLayer LayerName="Main" />
//
// 어느 쪽이든 어셈블리명은 드러나지 않으므로, 플랫폼을 옮겨도 고칠 것이 없다.

#if WPF || OPENSILVER
using System.Windows.Markup;

[assembly: XmlnsPrefix(Jamesnet.Foundation.XamlNamespace.Uri, "james")]
[assembly: XmlnsDefinition(Jamesnet.Foundation.XamlNamespace.Uri, "Jamesnet.Foundation")]
[assembly: XmlnsDefinition(Jamesnet.Foundation.XamlNamespace.Uri, "Jamesnet.Foundation.Responsive")]

// 9.x 시절의 플랫폼별 네임스페이스에 남아 있는 타입들(AnimatedLayer, Docking 등)도
// 같은 URI로 함께 노출한다. 소비자는 이 둘을 구분할 필요가 없다.
#if WPF
[assembly: XmlnsDefinition(Jamesnet.Foundation.XamlNamespace.Uri, "Jamesnet.Platform.Wpf")]
#else
[assembly: XmlnsDefinition(Jamesnet.Foundation.XamlNamespace.Uri, "Jamesnet.Platform.OpenSilver")]
#endif

#endif

namespace Jamesnet.Foundation
{
    /// <summary>
    /// Jamesnet의 XAML 네임스페이스 URI. WPF와 OpenSilver에서 사용한다.
    /// Uno, WinUI3, UWP에서는 <c>using:Jamesnet.Foundation</c>을 쓴다.
    /// </summary>
    public static class XamlNamespace
    {
        public const string Uri = "http://jamesnet.dev/xaml";
    }
}
