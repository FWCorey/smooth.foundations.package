using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Smooth.Algebraics;
using Smooth.Delegates;
using Smooth.Slinq;

#if !UNITY_3_5
namespace Smooth.Slinq.Test {
#endif

	/// <summary>
	/// Test controller and (de)verifier for Smooth.Slinq.
	/// </summary>
	public class SlinqTest : MonoBehaviour {
		public static readonly DelegateFunc<ValueTuple<int, int>, ValueTuple<int, int>, bool> eq = (a, b) => a == b;
		public static readonly DelegateFunc<ValueTuple<int, int, int>, ValueTuple<int, int, int>, bool> eq_t3 = (a, b) => a == b;

		public static readonly DelegateFunc<ValueTuple<int, int>, int> to_1 = t => t.Item1;
		public static readonly Func<ValueTuple<int, int>, int> to_1f = t => t.Item1;

		public static readonly IEqualityComparer<ValueTuple<int, int>> eq_1 = new Equals_1<int, int>();
		
		public static readonly StringBuilder stringBuilder = new StringBuilder();

		public static List<ValueTuple<int, int>> tuples1 = new List<ValueTuple<int, int>>();
		public static List<ValueTuple<int, int>> tuples2 = new List<ValueTuple<int, int>>();

		public static int loops = 1;

		public int minCount = 50;
		public int maxCount = 100;

		public int minValue = 1;
		public int maxValue = 100;

		public int speedLoops = 10;
	
		public bool testCorrectness;

		private void Start() {
			tuples1 = new List<ValueTuple<int, int>>(maxCount);
			tuples2 = new List<ValueTuple<int, int>>(maxCount);
			loops = speedLoops;

			Debug.Log("Element count: " + minCount + " to " + maxCount + ", value range: " + minValue + " to " + maxValue + ", loops: " + loops);
			
			if (testCorrectness) {
				Debug.Log("Testing Correctness.");
			}
		}

		private void Update() {
			if (testCorrectness) {
				TestCorrectness();
			}
		}

		private void LateUpdate() {
			loops = speedLoops;

			tuples1.Clear();
			tuples2.Clear();

			var count = UnityEngine.Random.Range(minCount, maxCount + 1);
			for (int i = 0; i < count; ++i) {
				tuples1.Add(new ValueTuple<int, int>(UnityEngine.Random.Range(minValue, maxValue + 1), i));
				tuples2.Add(new ValueTuple<int, int>(UnityEngine.Random.Range(minValue, maxValue + 1), i));
			}
		}

		private void TestCorrectness() {
			var testTuple = tuples2.Slinq().FirstOrDefault();
			var testInt = testTuple.Item1;
			var testInt2 = testInt * (maxValue - minValue + 1) / 25;
			var midSkip = UnityEngine.Random.value < 0.5f ? testInt : 0;

			if (tuples1.Slinq().Aggregate(0, (acc, next) => acc + next.Item1) != tuples1.Aggregate(0, (acc, next) => acc + next.Item1)) {
				Debug.LogError("Aggregate failed.");
				testCorrectness = false;
			}

			if (tuples1.Slinq().Aggregate(0, (acc, next) => acc + next.Item1, acc => -acc) != tuples1.Aggregate(0, (acc, next) => acc + next.Item1, acc => -acc)) {
				Debug.LogError("Aggregate failed.");
				testCorrectness = false;
			}
			
			if (tuples1.Slinq().AggregateWhile(0, (acc, next) => acc < testInt2 ? new Option<int>(acc + next.Item1) : new Option<int>()) != tuples1.Slinq().AggregateRunning(0, (acc, next) => acc + next.Item1).Where(acc => acc >= testInt2).FirstOrDefault()) {
				Debug.LogError("AggregateWhile / AggregateRunning failed.");
				testCorrectness = false;
			}

			if (tuples1.Slinq().All(x => x.Item1 < testInt2) ^ tuples1.All(x => x.Item1 < testInt2)) {
				Debug.LogError("All failed.");
				testCorrectness = false;
			}

			if (tuples1.Slinq().All((x, c) => x.Item1 < c, testInt2) ^ tuples1.All(x => x.Item1 < testInt2)) {
				Debug.LogError("All failed.");
				testCorrectness = false;
			}

			if (tuples1.Slinq().Any(x => x.Item1 > testInt2) ^ tuples1.Any(x => x.Item1 > testInt2)) {
				Debug.LogError("All failed.");
				testCorrectness = false;
			}
			
			if (tuples1.Slinq().Any((x, c) => x.Item1 > c, testInt2) ^ tuples1.Any(x => x.Item1 > testInt2)) {
				Debug.LogError("All failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().Select(to_1).AverageOrNone().Cata(avg => avg == tuples1.Select(to_1f).Average(), tuples1.Count == 0)) {
				Debug.LogError("AverageOrNone failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().Concat(tuples2.Slinq()).SequenceEqual(tuples1.Concat(tuples2).Slinq(), eq)) {
				Debug.LogError("Concat failed.");
				testCorrectness = false;
			}
			
			if (tuples1.Slinq().Contains(testTuple, eq_1) ^ tuples1.Contains(testTuple, eq_1)) {
				Debug.LogError("Contains failed.");
				testCorrectness = false;
			}
			
			if (tuples1.Slinq().Where(x => x.Item1 < testInt).Count() != tuples1.Where(x => x.Item1 < testInt).Count()) {
				Debug.LogError("Count failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().Distinct(eq_1).SequenceEqual(tuples1.Distinct(eq_1).Slinq(), eq)) {
				Debug.LogError("Distinct failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().Except(tuples2.Slinq(), eq_1).SequenceEqual(tuples1.Except(tuples2, eq_1).Slinq(), eq)) {
				Debug.LogError("Except failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().FirstOrNone().Cata(x => x == tuples1.First(), !tuples1.Any())) {
				Debug.LogError("FirstOrNone failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().FirstOrNone(x => x.Item1 < testInt).Cata(x => x == tuples1.First(z => z.Item1 < testInt), !tuples1.Where(z => z.Item1 < testInt).Any())) {
				Debug.LogError("FirstOrNone(predicate) failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().FirstOrNone((x, t) => x.Item1 < t, testInt).Cata(x => x == tuples1.First(z => z.Item1 < testInt), !tuples1.Where(z => z.Item1 < testInt).Any())) {
				Debug.LogError("FirstOrNone(predicate, parameter) failed.");
				testCorrectness = false;
			}
			
			if (!new List<ValueTuple<int, int>>[] { tuples1, tuples2 }.Slinq().Select(x => x.Slinq()).Flatten().SequenceEqual(tuples1.Concat(tuples2).Slinq(), eq)) {
				Debug.LogError("Flatten failed.");
				testCorrectness = false;
			}

			{
				var feAcc = 0;
				tuples1.Slinq().ForEach(x => feAcc += x.Item1);
				if (feAcc != tuples1.Slinq().Select(to_1).Sum()) {
					Debug.LogError("ForEach failed.");
					testCorrectness = false;
				}
			}

			if (!tuples1.Slinq().Intersect(tuples2.Slinq(), eq_1).SequenceEqual(tuples1.Intersect(tuples2, eq_1).Slinq(), eq)) {
				Debug.LogError("Intersect failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().GroupBy(to_1).SequenceEqual(
				tuples1.GroupBy(to_1f).Slinq(),
				(s, l) => s.key == l.Key && s.values.SequenceEqual(l.Slinq(), eq))) {
				Debug.LogError("GroupBy failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().GroupJoin(tuples2.Slinq(), to_1, to_1, (a, bs) => ValueTuple.Create(a.Item1, a.Item2, bs.Count()))
			    .SequenceEqual(tuples1.GroupJoin(tuples2, to_1f, to_1f, (a, bs) => ValueTuple.Create(a.Item1, a.Item2, bs.Count())).Slinq(), eq_t3)) {
				Debug.LogError("GroupJoin failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().Join(tuples2.Slinq(), to_1, to_1, (a, b) => ValueTuple.Create(a.Item1, a.Item2, b.Item2))
			    .SequenceEqual(tuples1.Join(tuples2, to_1f, to_1f, (a, b) => ValueTuple.Create(a.Item1, a.Item2, b.Item2)).Slinq(), eq_t3)) {
				Debug.LogError("Join failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().LastOrNone().Cata(x => x == tuples1.Last(), !tuples1.Any())) {
				Debug.LogError("LastOrNone failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().LastOrNone(x => x.Item1 < testInt).Cata(x => x == tuples1.Last(z => z.Item1 < testInt), !tuples1.Where(z => z.Item1 < testInt).Any())) {
				Debug.LogError("LastOrNone(predicate) failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().LastOrNone((x, t) => x.Item1 < t, testInt).Cata(x => x == tuples1.Last(z => z.Item1 < testInt), !tuples1.Where(z => z.Item1 < testInt).Any())) {
				Debug.LogError("LastOrNone(predicate, parameter) failed.");
				testCorrectness = false;
			}
			
			if (tuples1.Count > 0 && tuples1.Slinq().Max() != tuples1.Max()) {
				Debug.LogError("Max failed.");
				testCorrectness = false;
			}

			if (tuples1.Count > 0 && tuples1.Slinq().Min() != tuples1.Min()) {
				Debug.LogError("Min failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().MaxOrNone().Cata(x => x == tuples1.Max(), tuples1.Count == 0)) {
				Debug.LogError("MaxOrNone failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().MinOrNone().Cata(x => x == tuples1.Min(), tuples1.Count == 0)) {
				Debug.LogError("MinOrNone failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().OrderBy(to_1).SequenceEqual(tuples1.OrderBy(to_1f).Slinq(), eq)) {
				Debug.LogError("OrderBy failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().OrderByDescending(to_1).SequenceEqual(tuples1.OrderByDescending(to_1f).Slinq(), eq)) {
				Debug.LogError("OrderByDescending failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().OrderBy().SequenceEqual(tuples1.OrderBy(x => x).Slinq(), eq)) {
				Debug.LogError("OrderBy keyless failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().OrderByDescending().SequenceEqual(tuples1.OrderByDescending(x => x).Slinq(), eq)) {
				Debug.LogError("OrderByDescending keyless failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().OrderByGroup(to_1).SequenceEqual(tuples1.OrderBy(to_1f).Slinq(), eq)) {
				Debug.LogError("OrderByGroup failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().OrderByGroupDescending(to_1).SequenceEqual(tuples1.OrderByDescending(to_1f).Slinq(), eq)) {
				Debug.LogError("OrderByGroupDescending failed.");
				testCorrectness = false;
			}
			
			{
				var list = RemovableList();
				var slinq = list.Slinq().Skip(midSkip);
				for (int i = 0; i < testInt; ++i) {
					slinq.Remove();
				}
				if (!list.Slinq().Skip(midSkip).SequenceEqual(tuples1.Slinq().Skip(midSkip).Skip(testInt), eq)) {
					Debug.LogError("Remove failed.");
					testCorrectness = false;
				}
			}
			
			{
				var list = RemovableList();
				var slinq = list.SlinqDescending().Skip(midSkip);
				for (int i = 0; i < testInt; ++i) {
					slinq.Remove();
				}
				if (!list.SlinqDescending().Skip(midSkip).SequenceEqual(tuples1.SlinqDescending().Skip(midSkip).Skip(testInt), eq)) {
					Debug.LogError("Remove descending failed.");
					testCorrectness = false;
				}
			}

			{
				var list = RemovableList();
				list.Slinq().Skip(midSkip).Remove(testInt);
				if (!list.Slinq().Skip(midSkip).SequenceEqual(tuples1.Slinq().Skip(midSkip).Skip(testInt), eq)) {
					Debug.LogError("Remove(int) failed.");
					testCorrectness = false;
				}
			}
			
			{
				var list = RemovableList();
				list.Slinq().Skip(midSkip).RemoveWhile(x => x.Item1 < testInt2);
				if (!list.Slinq().Skip(midSkip).SequenceEqual(tuples1.Slinq().Skip(midSkip).SkipWhile(x => x.Item1 < testInt2), eq)) {
					Debug.LogError("RemoveWhile failed.");
					testCorrectness = false;
				}
			}

			{
				var list = RemovableList();
				var sSlinq = tuples1.Slinq().Skip(midSkip);
				var rAcc = list.Slinq().Skip(midSkip).RemoveWhile(0, (acc, next) => acc < testInt2 ? new Option<int>(acc + next.Item1) : new Option<int>());
				var sAcc = sSlinq.SkipWhile(0, (acc, next) => acc < testInt2 ? new Option<int>(acc + next.Item1) : new Option<int>());

				if (rAcc != sAcc || !list.Slinq().Skip(midSkip).SequenceEqual(sSlinq, eq)) {
					Debug.LogError("RemoveWhile aggregating failed.");
					testCorrectness = false;
				}
			}
			
			
			{
				var list = RemovableLinkedList();
				var slinq = list.Slinq().Skip(midSkip);
				for (int i = 0; i < testInt; ++i) {
					slinq.Remove();
				}
				if (!list.Slinq().Skip(midSkip).SequenceEqual(tuples1.Slinq().Skip(midSkip).Skip(testInt), eq)) {
					Debug.LogError("Remove LL failed.");
					testCorrectness = false;
				}
			}
			
			
			{
				var list = RemovableLinkedList();
				var slinq = list.SlinqDescending().Skip(midSkip);
				for (int i = 0; i < testInt; ++i) {
					slinq.Remove();
				}
				if (!list.SlinqDescending().Skip(midSkip).SequenceEqual(tuples1.SlinqDescending().Skip(midSkip).Skip(testInt), eq)) {
					Debug.LogError("Remove descending LL failed.");
					testCorrectness = false;
				}
			}
			
			{
				var list = RemovableLinkedList();
				list.Slinq().Skip(midSkip).Remove(testInt);
				if (!list.Slinq().Skip(midSkip).SequenceEqual(tuples1.Slinq().Skip(midSkip).Skip(testInt), eq)) {
					Debug.LogError("Remove(int) LL failed.");
					testCorrectness = false;
				}
			}
			
			{
				var list = RemovableLinkedList();
				list.Slinq().Skip(midSkip).RemoveWhile(x => x.Item1 < testInt2);
				if (!list.Slinq().Skip(midSkip).SequenceEqual(tuples1.Slinq().Skip(midSkip).SkipWhile(x => x.Item1 < testInt2), eq)) {
					Debug.LogError("RemoveWhile LL failed.");
					testCorrectness = false;
				}
			}
			
			{
				var list = RemovableLinkedList();
				var sSlinq = tuples1.Slinq().Skip(midSkip);
				var rAcc = list.Slinq().Skip(midSkip).RemoveWhile(0, (acc, next) => acc < testInt2 ? new Option<int>(acc + next.Item1) : new Option<int>());
				var sAcc = sSlinq.SkipWhile(0, (acc, next) => acc < testInt2 ? new Option<int>(acc + next.Item1) : new Option<int>());
				
				if (rAcc != sAcc || !list.Slinq().Skip(midSkip).SequenceEqual(sSlinq, eq)) {
					Debug.LogError("RemoveWhile aggregating LL failed.");
					testCorrectness = false;
				}
			}

			if (!tuples1.Slinq().Reverse().SequenceEqual(Enumerable.Reverse(tuples1).Slinq(), eq)) {
				Debug.LogError("Reverse failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().Select(to_1).SequenceEqual(tuples1.Select(to_1f).Slinq())) {
				Debug.LogError("Select failed.");
				testCorrectness = false;
			}
			
			if (!new List<ValueTuple<int, int>>[] { tuples1, tuples2 }.Slinq().SelectMany(x => x.Slinq()).SequenceEqual(tuples1.Concat(tuples2).Slinq(), eq)) {
				Debug.LogError("SelectMany failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().SelectMany(x => x.Item1 < testInt ? new Option<int>(x.Item1) : new Option<int>()).SequenceEqual(tuples1.Where(x => x.Item1 < testInt).Select(x => x.Item1).Slinq())) {
				Debug.LogError("SelectMany option failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().SequenceEqual(tuples1.Slinq(), eq)) {
				Debug.LogError("SequenceEqual failed.");
				testCorrectness = false;
			}
			
			if (tuples1.Slinq().SequenceEqual(tuples2.Slinq(), eq) ^ tuples1.SequenceEqual(tuples2)) {
				Debug.LogError("SequenceEqual failed.");
				testCorrectness = false;
			}
			
			{
				var a = new int[3];
				a[2] = testInt;
				if (a.Slinq().SingleOrNone().isSome ||
				    a.Slinq().Skip(1).SingleOrNone().isSome ||
				    !a.Slinq().Skip(2).SingleOrNone().Contains(testInt) ||
				    a.Slinq().Skip(3).SingleOrNone().isSome) {
					Debug.LogError("SingleOrNone failed.");
					testCorrectness = false;
				}
			}

			{
				var slinq = tuples1.Slinq();
				for (int i = 0; i < testInt; ++i) {
					slinq.Skip();
				}
				if (!tuples1.Slinq().Skip(testInt).SequenceEqual(slinq, eq)) {
					Debug.LogError("Skip failed.");
					testCorrectness = false;
				}
			}
			
			if (!tuples1.Slinq().SkipWhile(x => x.Item1 < testInt2).SequenceEqual(tuples1.SkipWhile(x => x.Item1 < testInt2).Slinq(), eq)) {
				Debug.LogError("SkipWhile failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().Reverse().SequenceEqual(tuples1.SlinqDescending(), eq)) {
				Debug.LogError("SlinqDescending failed.");
				testCorrectness = false;
			}

			if (!tuples1.SlinqDescending().SequenceEqual(RemovableLinkedList().SlinqDescending(), eq)) {
				Debug.LogError("SlinqDescending LL failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().SequenceEqual(RemovableLinkedList().SlinqNodes().Select(x => x.Value), eq)) {
				Debug.LogError("SlinqNodes LL failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.SlinqDescending().SequenceEqual(RemovableLinkedList().SlinqNodesDescending().Select(x => x.Value), eq)) {
				Debug.LogError("SlinqNodesDescending LL failed.");
				testCorrectness = false;
			}

			if (!tuples1.SlinqWithIndex().All(x => x.Item1.Item2 == x.Item2) ||
			    !tuples1.SlinqWithIndex().Select(x => x.Item1).SequenceEqual(tuples1.Slinq(), eq)) {
				Debug.LogError("SlinqWithIndex failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.SlinqWithIndexDescending().All(x => x.Item1.Item2 == x.Item2) ||
			    !tuples1.SlinqWithIndexDescending().Select(x => x.Item1).SequenceEqual(tuples1.SlinqDescending(), eq)) {
				Debug.LogError("SlinqWithIndexDescending failed.");
				testCorrectness = false;
			}

			if (tuples1.Slinq().Select(to_1).Sum() != tuples1.Select(to_1f).Sum()) {
				Debug.LogError("Sum failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().Take(testInt).SequenceEqual(tuples1.Take(testInt).Slinq(), eq)) {
				Debug.LogError("Take failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().TakeRight(testInt).SequenceEqual(tuples1.Slinq().Skip(tuples1.Count - testInt), eq)) {
				Debug.LogError("TakeRight failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().TakeWhile(x => x.Item1 < testInt2).SequenceEqual(tuples1.TakeWhile(x => x.Item1 < testInt2).Slinq(), eq)) {
				Debug.LogError("TakeWhile failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().TakeWhile((x, c) => x.Item1 < c, testInt2).SequenceEqual(tuples1.TakeWhile(x => x.Item1 < testInt2).Slinq(), eq)) {
				Debug.LogError("TakeWhile failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().Union(tuples2.Slinq(), eq_1).SequenceEqual(tuples1.Union(tuples2, eq_1).Slinq(), eq)) {
				Debug.LogError("Union failed.");
				testCorrectness = false;
			}
	
			if (!tuples1.Slinq().Where(x => x.Item1 < testInt).SequenceEqual(tuples1.Where(x => x.Item1 < testInt).Slinq(), eq)) {
				Debug.LogError("Where failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().Where((x, c) => x.Item1 < c, testInt).SequenceEqual(tuples1.Where(x => x.Item1 < testInt).Slinq(), eq)) {
				Debug.LogError("Where failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().Zip(tuples2.Slinq()).SequenceEqual(
				Slinqable.Sequence(0, 1)
				.TakeWhile(x => x < tuples1.Count && x < tuples2.Count)
				.Select(x => ValueTuple.Create(tuples1[x], tuples2[x])))) {
				Debug.LogError("Zip tuples failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().Zip(tuples2.Slinq(), (a, b) => ValueTuple.Create(a.Item1, b.Item1)).SequenceEqual(
				Slinqable.Sequence(0, 1)
				.TakeWhile(x => x < tuples1.Count && x < tuples2.Count)
				.Select(x => ValueTuple.Create(tuples1[x].Item1, tuples2[x].Item1)), eq)) {
				Debug.LogError("Zip failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq()
			    .ZipAll(tuples2.Slinq())
			    .SequenceEqual(Slinqable.Sequence(0, 1)
			               .TakeWhile(x => x < tuples1.Count || x < tuples2.Count)
			               .Select(x => ValueTuple.Create(
				x < tuples1.Count ?	new Option<ValueTuple<int, int>>(tuples1[x]) : new Option<ValueTuple<int, int>>(),
				x < tuples2.Count ?	new Option<ValueTuple<int, int>>(tuples2[x]) : new Option<ValueTuple<int, int>>())))) {
				Debug.LogError("ZipAll tuples failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().Skip(midSkip)
			    .ZipAll(tuples2.Slinq())
			    .SequenceEqual(Slinqable.Sequence(0, 1)
			               .TakeWhile(x => x + midSkip < tuples1.Count || x < tuples2.Count)
			               .Select(x => ValueTuple.Create(
				x + midSkip < tuples1.Count ? new Option<ValueTuple<int, int>>(tuples1[x + midSkip]) : new Option<ValueTuple<int, int>>(),
				x < tuples2.Count ? new Option<ValueTuple<int, int>>(tuples2[x]) : new Option<ValueTuple<int, int>>())))) {
				Debug.LogError("ZipAll tuples failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq()
			    .ZipAll(tuples2.Slinq().Skip(midSkip))
			    .SequenceEqual(Slinqable.Sequence(0, 1)
			               .TakeWhile(x => x < tuples1.Count || x + midSkip < tuples2.Count)
			               .Select(x => ValueTuple.Create(
				x < tuples1.Count ? new Option<ValueTuple<int, int>>(tuples1[x]) : new Option<ValueTuple<int, int>>(),
				x + midSkip < tuples2.Count ? new Option<ValueTuple<int, int>>(tuples2[x + midSkip]) : new Option<ValueTuple<int, int>>())))) {
				Debug.LogError("ZipAll tuples failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq()
			    .ZipAll(tuples2.Slinq(), (a, b) => ValueTuple.Create(a, b))
			    .SequenceEqual(Slinqable.Sequence(0, 1)
			               .TakeWhile(x => x < tuples1.Count || x < tuples2.Count)
			               .Select(x => ValueTuple.Create(
				x < tuples1.Count ?	new Option<ValueTuple<int, int>>(tuples1[x]) : new Option<ValueTuple<int, int>>(),
				x < tuples2.Count ?	new Option<ValueTuple<int, int>>(tuples2[x]) : new Option<ValueTuple<int, int>>())))) {
				Debug.LogError("ZipAll failed.");
				testCorrectness = false;
			}
			
			if (!tuples1.Slinq().Skip(midSkip)
			    .ZipAll(tuples2.Slinq(), (a, b) => ValueTuple.Create(a, b))
			    .SequenceEqual(Slinqable.Sequence(0, 1)
			               .TakeWhile(x => x + midSkip < tuples1.Count || x < tuples2.Count)
			               .Select(x => ValueTuple.Create(
				x + midSkip < tuples1.Count ? new Option<ValueTuple<int, int>>(tuples1[x + midSkip]) : new Option<ValueTuple<int, int>>(),
				x < tuples2.Count ? new Option<ValueTuple<int, int>>(tuples2[x]) : new Option<ValueTuple<int, int>>())))) {
				Debug.LogError("ZipAll failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq()
			    .ZipAll(tuples2.Slinq().Skip(midSkip), (a, b) => ValueTuple.Create(a, b))
			    .SequenceEqual(Slinqable.Sequence(0, 1)
			               .TakeWhile(x => x < tuples1.Count || x + midSkip < tuples2.Count)
			               .Select(x => ValueTuple.Create(
				x < tuples1.Count ? new Option<ValueTuple<int, int>>(tuples1[x]) : new Option<ValueTuple<int, int>>(),
				x + midSkip < tuples2.Count ? new Option<ValueTuple<int, int>>(tuples2[x + midSkip]) : new Option<ValueTuple<int, int>>())))) {
				Debug.LogError("ZipAll failed.");
				testCorrectness = false;
			}

			if (!tuples1.Slinq().ZipWithIndex().All(x => x.Item1.Item2 == x.Item2) ||
			    !tuples1.Slinq().ZipWithIndex().Select(x => x.Item1).SequenceEqual(tuples1.Slinq(), eq)) {
				Debug.LogError("ZipWithIndex failed.");
				testCorrectness = false;
			}
		}

		private List<ValueTuple<int, int>> RemovableList() {
			return new List<ValueTuple<int, int>>(tuples1);
		}

		private LinkedList<ValueTuple<int, int>> RemovableLinkedList() {
			return new LinkedList<ValueTuple<int, int>>(tuples1);
		}

		public class Equals_1<T1, T2> : IEquatable<Equals_1<T1, T2>>, IEqualityComparer<ValueTuple<T1, T2>> {
			public readonly IEqualityComparer<T1> equalityComparer;

			public Equals_1() {
				this.equalityComparer = Smooth.Collections.EqualityComparer<T1>.Default;
			}

			public Equals_1(IEqualityComparer<T1> equalityComparer) {
				this.equalityComparer = equalityComparer;
			}
			
			public override bool Equals(object o) {
				return o is Equals_1<T1, T2> && this.Equals((Equals_1<T1, T2>) o);
			}
			
			public bool Equals(Equals_1<T1, T2> other) {
				return this.equalityComparer == other.equalityComparer;
			}
			
			public override int GetHashCode() {
				return this.equalityComparer.GetHashCode();
			}
			
			public static bool operator == (Equals_1<T1, T2> lhs, Equals_1<T1, T2> rhs) {
				return lhs.Equals(rhs);
			}
			
			public static bool operator != (Equals_1<T1, T2> lhs, Equals_1<T1, T2> rhs) {
				return !lhs.Equals(rhs);
			}

			public bool Equals(ValueTuple<T1, T2> a, ValueTuple<T1, T2> b) {
				return equalityComparer.Equals(a.Item1, b.Item1);
			}

			public int GetHashCode(ValueTuple<T1, T2> a) {
				return equalityComparer.GetHashCode(a.Item1);
			}
		}
	}

#if !UNITY_3_5
}
#endif
