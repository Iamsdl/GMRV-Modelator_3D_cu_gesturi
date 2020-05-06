using Hover.Core.Items.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ParticlesController : MonoBehaviour
{
    public HoverItemDataSelector AddParticlesButton;

    public HoverItemDataSelector SmokeTextureButton;
    public HoverItemDataSelector FireTextureButton;

    public HoverItemDataSlider IntensitySlider;
    public HoverItemDataSlider DragSlider;
    public HoverItemDataSlider SizeSlider;

    public Texture2D SmokeTexture;
    public Texture2D FireTexture;

    public GameObject fieldPos;
    public VisualEffect Particles;
    Vector3[] points;

    private AnimationCurve animationCurve;

    // Start is called before the first frame update
    void Start()
    {
        SmokeTextureButton.OnSelected += SmokeTextureButton_OnSelected;
        FireTextureButton.OnSelected += FireTextureButton_OnSelected;
        IntensitySlider.OnValueChanged += IntensitySlider_OnValueChanged;
        DragSlider.OnValueChanged += DragSlider_OnValueChanged;
        SizeSlider.OnValueChanged += SizeSlider_OnValueChanged;
    }

    private void SizeSlider_OnValueChanged(IItemDataSelectable<float> pItem)
    {
        animationCurve = AnimationCurve.Constant(Particles.GetFloat("Min lifetime"), Particles.GetFloat("Max lifetime"), SizeSlider.Value);
        Particles.SetAnimationCurve("Particle size", animationCurve);
    }

    private void DragSlider_OnValueChanged(IItemDataSelectable<float> pItem)
    {
        Particles.SetFloat("Field drag", DragSlider.Value);
    }

    private void IntensitySlider_OnValueChanged(IItemDataSelectable<float> pItem)
    {
        Particles.SetFloat("Field intensity", IntensitySlider.Value);
    }

    private void SmokeTextureButton_OnSelected(IItemDataSelectable pItem)
    {
        Particles.SetTexture("Particle texture", SmokeTexture);
    }

    private void FireTextureButton_OnSelected(IItemDataSelectable pItem)
    {
        Particles.SetTexture("Particle texture", FireTexture);
    }


    // Update is called once per frame
    void Update()
    {

    }
    public void StopDrawingPath(LineRenderer lineRenderer)
    {
        points = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(points);


        Particles.transform.position = points[0];
        Vector3 min = TextureGenerator.GetMin(points);
        Vector3 max = TextureGenerator.GetMax(points);
        fieldPos.transform.position = (min + max) / 2;
        Particles.SetVector3("FieldScale", (max - min) * 2);

        Texture3D texture = TextureGenerator.CreateTexture3D(32, points);

        Particles.SetTexture("VectorField", texture);
    }
}
