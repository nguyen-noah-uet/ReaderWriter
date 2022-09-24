namespace ReaderWriter;

public class Program
{
	static void Main(string[] args)
	{
		var readerWriter = new ReaderWriter();
		Thread writerThread = new Thread(readerWriter.Write);
		Thread readerThread1 = new Thread(readerWriter.Read);
		Thread readerThread2 = new Thread(readerWriter.Read);
		//Thread readerThread3 = new Thread(readerWriter.Read);
		writerThread.Start();
		readerThread1.Start();
		readerThread2.Start();
		//readerThread3.Start();

	}
}
public class ReaderWriter
{
	private Semaphore _mutex;
	private Semaphore _write;
	private int readerCount = 0;
	private DateTime _start;
	public ReaderWriter()
	{
		_mutex = new Semaphore(initialCount: 1, maximumCount: 1);
		_write = new Semaphore(initialCount: 1, maximumCount: 1);
		_start = DateTime.Now;
	}
	public void Read()
	{
		do
		{
			_mutex.WaitOne(); // acquire lock to increment readerCount by 1.
			readerCount += 1;
			// when we are reading, no one should be allowed to write, thus the wrt semaphore should be acquired;
			// so if we are the first reader, we acquire it (and wait till it's released if someone's still writing);
			// but if someone's already reading, it means they have already acquired _write, so we just skip this part.
			if (readerCount == 1)
			{
				_write.WaitOne();
			}
			_mutex.Release(); // release lock, other readers can enter while current reader is in its critical section

			Console.WriteLine("Reading the Db...\t readerCount = " + readerCount);
			Thread.Sleep(1000);

			_mutex.WaitOne(); // acquire lock to decrement readerCount by 1.
			readerCount -= 1; // reader wants to leave.
			if (readerCount == 0) // if there is no reader, writer can enter.
			{
				_write.Release();
			}
			_mutex.Release(); // release lock.

		} while ((DateTime.Now - _start).Seconds < 10);
	}

	public void Write()
	{
		do
		{
			// Writer request for critical section.
			_write.WaitOne();

			#region CritialSection
			// Do the write operation
			Console.WriteLine("Updating the Db...");
			Thread.Sleep(1000);
			#endregion

			// Leave critical section.
			_write.Release();
		} while ((DateTime.Now - _start).Seconds < 10);
	}
}