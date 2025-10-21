using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StampRallyUI : MonoBehaviour
{
    [System.Serializable]
    public class GenreStamp
    {
        public TourTokyo.Genre Genre;
        public Sprite Icon;
        public Sprite Gray;
        public GameObject Group;
        public int CompletionPoints = 100;
        private Image[] images;
        private int i = 0;
        private AudioClip clip;
        public void Initialize(AudioClip clip)
        {
            this.clip = clip;
            images = Group.GetComponentsInChildren<Image>().Where(c => c.gameObject != Group).ToArray();
            foreach (Image img in images) { img.sprite = Gray; }
        }

        public IEnumerator ActivateNext(Action callback)
        {
            if (!GenreComplete())
            {
                Vector3 origScale = images[i].transform.localScale;
                // Quaternion origRot = images[i].transform.localRotation;
                float t = 0f;
                float startScale = 2f;
                float finalScale = 1f;
                float startRot = 90f;
                float finalRot = 0f;
                images[i].sprite = Icon;
                // Quaternion variableRot
                while (t < 1f)
                {
                    t += Time.deltaTime * 2f;
                    images[i].transform.localScale = origScale * (startScale + t * (finalScale - startScale));
                    // images[i].transform.localRotation = ;
                    yield return null;
                }
                images[i].transform.localScale = origScale;       // settle
                AudioController.PlayClip(clip);
                i++;
            }
            callback();
        }

        public int ActivatedCount()
        {
            return i + 1;
        }
        public bool GenreComplete()
        {
            return i == images.Length;
        }
    }

    [SerializeField] private GenreStamp[] stamps;

    [SerializeField] private AudioClip stampSound;
    public static Dictionary<TourTokyo.Genre, GenreStamp> GenreStamps = new Dictionary<TourTokyo.Genre, GenreStamp>();
    // Start is called before the first frame update
    void Start()
    {
        foreach (GenreStamp stamp in stamps)
        {
            stamp.Initialize(stampSound);
            GenreStamps[stamp.Genre] = stamp;
        }
    }

}
