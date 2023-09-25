using System;
using static UnityEditor.Experimental.GraphView.Port;

namespace Nebula
{
    public class WeightedSelection<T>
    {
        private const int MIN_CAPACITY = 8;
        public struct ChoiceInformation
        {
            public T value;
            public float weight;
        }

        public ChoiceInformation[] choices;

        public int Count { get; private set; }
        public int Capacity
        {
            get => choices.Length;
            set
            {
                if(value < 8 || value < Count)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                ChoiceInformation[] currentChoices = choices;
                choices = new ChoiceInformation[value];
                Array.Copy(currentChoices, choices, Count);
            }
        }
        private float totalWeight;

        public WeightedSelection(int capacity = MIN_CAPACITY)
        {
            choices = new ChoiceInformation[capacity];
        }

        public WeightedSelection(WeightedSelection<T> orig)
        {
            choices = new ChoiceInformation[orig.Capacity];
            for (int i = 0; i < orig.Count; i++)
            {
                AddChoice(orig.GetChoice(i));
            }
        }

        public void AddChoice(T value, float choiceWeight)
        {
            AddChoice(new ChoiceInformation
            {
                value = value,
                weight = choiceWeight
            });
        }

        public void AddChoice(ChoiceInformation choiceInfo)
        {
            if (Count == Capacity)
                Capacity *= 2;

            choices[Count++] = choiceInfo;
            totalWeight += choiceInfo.weight;
        }

        public void RemoveChoice(int choiceIndex)
        {
            ThrowIfOutOfRange(choiceIndex, nameof(choiceIndex));
            int i = choiceIndex;

            for(int num = Count - 1; i < num; i++)
            {
                choices[i] = choices[i + 1];
            }
            choices[--Count] = default;
            RecalculateTotalWeight();
        }

        public void ModifyChoiceWeight(int choiceIndex, float newWeight)
        {
            ThrowIfOutOfRange(choiceIndex, nameof(choiceIndex));
            choices[choiceIndex].weight = newWeight;
            RecalculateTotalWeight();
        }

        public void RecalculateTotalWeight()
        {
            totalWeight = 0;
            for(int i = 0; i < Count; i++)
            {
                totalWeight += choices[i].weight;
            }
        }

        public void Clear()
        {
            for(int i = 0; i < Count; i++)
            {
                choices[i] = default;
            }
            Count = 0;
            totalWeight = 0;
        }
        public T Evaluate(float normalizedIndex)
        {
            return Evaluate(normalizedIndex, null);
        }

        public T Evaluate(float normalizedIndex, int[] ignoredIndices)
        {
            int index = EvaluateToChoiceIndex(normalizedIndex, ignoredIndices);
            if (index == -1)
                return default;
            return choices[index].value;
        }

        public int EvaluateToChoiceIndex(float normalizedIndex, int[] ignoredIndices)
        {
            if (Count == 0)
                return -1;

            float evaluationTotalWeight = totalWeight;
            if(ignoredIndices != null)
            {
                foreach(int ignoredIndex in ignoredIndices)
                {
                    evaluationTotalWeight -= choices[ignoredIndex].weight;
                }
            }

            float num = normalizedIndex * evaluationTotalWeight;
            float num1 = 0;
            for(int i = 0; i < Count; i++)
            {
                if(ignoredIndices == null || Array.IndexOf(ignoredIndices, i) == -1)
                {
                    num1 += choices[i].weight;
                    if (num1 < num)
                        return i;
                }
            }
            return Count - 1;
        }

        public ChoiceInformation GetChoice(int i)
        {
            return choices[i];
        }
        private void ThrowIfOutOfRange(int index, string paramName)
        {
            if (index < 0 || Count <= index)
                throw new ArgumentOutOfRangeException(paramName);
        }
    }
}