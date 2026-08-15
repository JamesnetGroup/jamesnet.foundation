using System;
using Jamesnet.Foundation;

namespace Jamesnet.Platform.Uno;

/// <summary>
/// 9.x 호환용 별칭. 구현은 <see cref="XamlLayer"/> 한 곳에만 있다.
/// </summary>
[Obsolete("Jamesnet.Foundation.XamlLayer를 사용하세요. UnoLayer는 다음 메이저 버전에서 제거됩니다.")]
public class UnoLayer : XamlLayer
{
}
