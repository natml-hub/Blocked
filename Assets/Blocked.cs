/* 
*   Blocked
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using NatSuite.Devices;
    using NatSuite.ML;
    using NatSuite.ML.Vision.Meet;

    public class Blocked : MonoBehaviour {

        [Header("Rendering")]
        public Shader shader;
        [Range(50, 250)] public float strength = 100;
        
        [Header(@"UI")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        Texture2D previewTexture;
        RenderTexture blockTexture;
        Material blockMaterial;

        MLModelData modelData;
        MLModel model;
        MeetSegmentationPredictor predictor;

        async void Start () {
            // Request camera permissions
            if (!await MediaDeviceQuery.RequestPermissions<CameraDevice>()) {
                Debug.LogError("User did not grant camera permissions");
                return;
            }
            // Start the camera preview
            var query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
            var device = query.current as CameraDevice;
            previewTexture = await device.StartRunning();
            // Create block texture
            blockTexture = new RenderTexture(previewTexture.width, previewTexture.height, 0);
            blockMaterial = new Material(shader);
            // Create the segmentation predictor
            modelData = await MLModelData.FromHub("@natsuite/meet-segmentation", "<HUB ACCESS KEY>");
            model = modelData.Deserialize();
            predictor = new MeetSegmentationPredictor(model);
            // Display segmentation image
            rawImage.texture = blockTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
        }

        void Update () {
            // Check that the predictor has been created
            if (predictor == null)
                return;
            // Predict
            var segmentationImage = RenderTexture.GetTemporary(blockTexture.descriptor);
            var segmentationMap = predictor.Predict(previewTexture);
            segmentationMap.Render(segmentationImage);
            // Render block
            var aspect = new Vector2(1f, (float)Screen.height / Screen.width);
            blockMaterial.SetFloat("_Strength", strength);
            blockMaterial.SetVector("_Aspect", aspect);
            blockMaterial.SetTexture("_Map", segmentationImage);
            Graphics.Blit(previewTexture, blockTexture, blockMaterial);
            RenderTexture.ReleaseTemporary(segmentationImage);
        }
    }
}