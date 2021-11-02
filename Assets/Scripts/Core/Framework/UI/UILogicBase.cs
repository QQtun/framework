//namespace Core.Framework.UI
//{
//    public interface IUILogicImpl
//    {
//        void OnOpen(object param);
//        void OnClose();
//    }

//    public abstract class UILogicBase : IUILogicImpl
//    {
//        public string Name { get; private set; }
//        public UIRoot Root { get; private set; }
//        public UILogicBase(string name, UIRoot root)
//        {
//            Name = name;
//            Root = root;
//        }

//        public abstract void OnClose();
//        public abstract void OnOpen(object param);

//        public void CloseSelf()
//        {
//            UIManager.Instance.Close(Name);
//        }
//    }
//}