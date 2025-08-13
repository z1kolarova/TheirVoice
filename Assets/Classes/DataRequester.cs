using System.Collections.Generic;

namespace Assets.Classes
{
    public class DataRequester
    {
        private bool isCurrentlyWaiting = false;
        public bool IsCurrentlyWaiting() => isCurrentlyWaiting;

        private bool needsData = false;
        public bool NeedsData() => needsData;

        public void RequestData() 
        {  
            needsData = true; 
            isCurrentlyWaiting = true;
        }

        public void UpdateBeganProcessing() { needsData = false; }
        public void UpdateDataReceivedAndProcessed() { isCurrentlyWaiting = false; }
    }

    public class MultiDataRequester<T>
    {
        private Dictionary<T, bool> currentlyWaitedForDic = new Dictionary<T, bool>();
        public bool IsCurrentlyWaiting(T idKey)
            => currentlyWaitedForDic.ContainsKey(idKey)
            && currentlyWaitedForDic[idKey];

        private List<T> newNeededData = new List<T>();
        public List<T> DataKeyQueue() => newNeededData;

        public bool NeedsNewData() => newNeededData.Count > 0;
        public void RequestData(T idKey)
        {
            currentlyWaitedForDic[idKey] = true;
            newNeededData.Add(idKey);
        }

        public void UpdateBeganProcessing(T idKey) { newNeededData.Remove(idKey); }
        public void UpdateDataReceivedAndProcessed(T idKey) { currentlyWaitedForDic[idKey] = false; }
    }
}
