using NUnit.Framework;
using System.Threading.Tasks;

namespace TCGE.Tests
{
    public class AtomicIntTests
    {
        [Test]
        [Category("Atomic")]
        public void AtomicInt_Increment()
        {
            AtomicInt counter = new AtomicInt();
            Assert.That(counter.value, Is.EqualTo(0));

            counter++;
            Assert.That(counter.value, Is.EqualTo(1));

            counter--;
            Assert.That(counter.value, Is.EqualTo(0));

            Task[] tasks = new Task[16];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => { counter++; });
            }

            Task.WhenAll(tasks).Wait();

            Assert.That(counter.value, Is.EqualTo(tasks.Length));
        }

        [Test]
        [Category("Atomic")]
        public void AtomicInt_Multiply()
        {
            const int testValue = 15;
            AtomicInt counter = new AtomicInt { value = testValue };
            Assert.That(counter.value, Is.EqualTo(testValue));

            counter *= 5;
            Assert.That(counter.value, Is.EqualTo(5 * testValue));

            counter /= 3;
            Assert.That(counter.value, Is.EqualTo((5 * testValue) / 3));

            counter = 1;

            Task[] tasks = new Task[16];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => { counter *= 2; });
            }

            Task.WhenAll(tasks).Wait();

            Assert.That(counter.value, Is.EqualTo(1 << tasks.Length));
        }

        [Test]
        [Category("Atomic")]
        public void AtomicInt_IncrementWrap()
        {
            AtomicInt counter = new AtomicInt();
            Assert.That(counter.value, Is.EqualTo(0));

            _ = counter.IncrementWrap(0, 2);
            Assert.That(counter.value, Is.EqualTo(1));

            _ = counter.IncrementWrap(0, 2);
            Assert.That(counter.value, Is.EqualTo(2));

            _ = counter.IncrementWrap(0, 2);
            Assert.That(counter.value, Is.EqualTo(0));

            counter = 1;

            Task[] tasks = new Task[16];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => { _ = counter.IncrementWrap(0, 15); });
            }

            Task.WhenAll(tasks).Wait();

            Assert.That(counter.value, Is.EqualTo(1));
        }
    }
}