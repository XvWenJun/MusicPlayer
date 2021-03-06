﻿using NaudioSource;
using UnityEngine;

public abstract class AudioVisualInfo : MonoBehaviour
{
    /// <summary>
    /// get audio source by tag named PlayingAudio
    /// </summary>
    private NaudioSources m_audio;

    /// <summary>
    /// initialize size on awake
    /// </summary>
    [SerializeField]
    private FFTSize m_spectrumSize = FFTSize.FFT1024;

    [SerializeField]
    private NaudioSource.FFTWindow m_fftWindow = NaudioSource.FFTWindow.HammingWindow;
    [SerializeField]
    private int m_samplesSpace = 20;

    /// <summary>
    /// must use UpdateSamples function before use this member
    /// </summary>
    private float[] m_leftChannelSamples;

    /// <summary>
    /// must use UpdateSamples function before use this member
    /// </summary>
    private float[] m_rightChannelSamples;

    /// <summary>
    /// for music visualize, left channel by 0, and right channel by 1
    /// </summary>
    private float[,] m_samples = null;

    private int m_samplesSize = 0;

    public float[,] Samples
    {
        get
        {
            return m_samples;
        }
    }

    /// <summary>
    /// initialization
    /// </summary>
    protected void Start()
    {
        ComponentsManager manager = GameObject.FindGameObjectWithTag(ComponentsManager.SELF_TAG).GetComponent<ComponentsManager>();
        m_audio = manager.m_audio;
        m_audio.BufferLength = m_spectrumSize;
        m_leftChannelSamples = new float[(int)m_spectrumSize];
        m_rightChannelSamples = new float[(int)m_spectrumSize];
    }

    private void FixedUpdate()
    {
        UpdateSamples();
        UpdateComponentsBySamples();
    }

    /// <summary>
    /// update samples data
    /// </summary>
    private void UpdateSamples()
    {
        try
        {
            m_audio.GetSample(m_leftChannelSamples, 0, m_fftWindow);
            m_audio.GetSample(m_rightChannelSamples, 1, m_fftWindow);
            AdjustSamples();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void AdjustSamples()
    {
        if (m_samplesSpace > (int)m_spectrumSize / m_samplesSize)
            m_samplesSpace = (int)m_spectrumSize / m_samplesSize;

        for (int i = 0; i < m_samplesSize; i++)
        {
            m_samples[0, i] = 0;
            m_samples[1, i] = 0;

            // cut off low rate, so use i + 2
            for (int j = (i + 4) * m_samplesSpace; j < (i + 5) * m_samplesSpace; j++)
            {
                if (j >= (int)m_spectrumSize)
                {
                    Debug.Log("Out of range");
                    break;
                }

                m_samples[0, i] += m_leftChannelSamples[j];
                m_samples[1, i] += m_rightChannelSamples[j];
            }
        }
    }

    protected void SetSamplesSize(int size)
    {
        m_samplesSize = size;
        if (m_samples == null)
        {
            m_samples = new float[2, m_samplesSize];
        }
    }

    protected abstract void CreateVisualComponents();

    protected abstract void UpdateComponentsBySamples();
}
