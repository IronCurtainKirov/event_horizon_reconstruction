namespace Services.Gui
{
    public enum WindowClass
    {
        HudElement,//自动打开
        TopLevel,//覆盖自身，关闭其他并禁止其他打开（除Balloon、ModalDialog）
        Singleton,//自动打开；覆盖自身并关闭Level2
        Level2,
        ModalDialog,//存在时禁止打开其他（除Balloon）；不存在自身时强制打开；强制不被关闭 【类似TopLevel，但优先级更高】
        Balloon,//强制打开；强制不被关闭；覆盖原有Balloon 【唯一；独立于其他】
    }

    public static class WindowClassExtensions
    {
        public static bool MustBeDisabledDueTo(this WindowClass myClass, WindowClass otherClass)
        {
            if (myClass == WindowClass.ModalDialog || myClass == WindowClass.Balloon)
                return false;
            if (otherClass == WindowClass.ModalDialog || otherClass == WindowClass.TopLevel)
                return true;

            return false;
        }

        public static bool MustBeClosedDueTo(this WindowClass myClass, WindowClass otherClass)
        {
            if (myClass == WindowClass.ModalDialog)
                return false;
            if (myClass == WindowClass.Balloon)
                return otherClass == WindowClass.Balloon;
            if (otherClass == WindowClass.TopLevel)
                return true;
            if (otherClass == WindowClass.Singleton)
                return myClass == WindowClass.Singleton || myClass == WindowClass.Level2;

            return false;
        }

        public static bool CantBeOpenedDueTo(this WindowClass myClass, WindowClass otherClass)
        {
            if (myClass == WindowClass.Balloon)
                return false;
            if (otherClass == WindowClass.ModalDialog)
                return true;
            if (myClass == WindowClass.ModalDialog)
                return false;
            if (otherClass == WindowClass.TopLevel)
                return myClass != WindowClass.TopLevel;

            return false;
        }

        public static bool MustBeOpenedAutomatically(this WindowClass windowClass)
        {
            return windowClass == WindowClass.Singleton || windowClass == WindowClass.HudElement;
        }
    }
}
