// 9.x 호환용 별칭. 기존 소비자 코드가 v10에서도 그대로 컴파일되도록 옛 이름만 남긴다.
// 구현은 모두 Jamesnet.Foundation 쪽 한 곳에 있다. 다음 메이저 버전에서 제거한다.
#pragma warning disable CS0618
using System;
using Jamesnet.Foundation;

#if WPF
namespace Jamesnet.Platform.Wpf
{
    [Obsolete("Jamesnet.Foundation.XamlLayer를 사용하세요.")]
    public class WPFLayer : XamlLayer { }

    [Obsolete("Jamesnet.Foundation.AnimatedXamlLayer를 사용하세요.")]
    public class AnimatedWPFLayer : AnimatedXamlLayer { }
}
#elif OPENSILVER
namespace Jamesnet.Platform.OpenSilver
{
    [Obsolete("Jamesnet.Foundation.XamlLayer를 사용하세요.")]
    public class OpenSilverLayer : XamlLayer { }

    [Obsolete("Jamesnet.Foundation.AnimatedXamlLayer를 사용하세요.")]
    public class AnimatedOpenSilverLayer : AnimatedXamlLayer { }
}
#elif UNO
namespace Jamesnet.Platform.Uno
{
    [Obsolete("Jamesnet.Foundation.XamlLayer를 사용하세요.")]
    public class UnoLayer : XamlLayer { }
}
#elif WINUI3
namespace Jamesnet.Platform.WinUI3
{
    [Obsolete("Jamesnet.Foundation.XamlLayer를 사용하세요.")]
    public class WinUI3Layer : XamlLayer { }

    [Obsolete("Jamesnet.Foundation.CustomWrapPanel을 사용하세요.")]
    public class CustomWrapPanel : Jamesnet.Foundation.CustomWrapPanel { }
}
#elif UWP
namespace Jamesnet.Platform.Uwp
{
    [Obsolete("Jamesnet.Foundation.XamlLayer를 사용하세요.")]
    public class UwpLayer : XamlLayer { }

    [Obsolete("Jamesnet.Foundation.CustomWrapPanel을 사용하세요.")]
    public class CustomWrapPanel : Jamesnet.Foundation.CustomWrapPanel { }
}
#endif
