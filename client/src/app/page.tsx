
import { listTransactions } from "@/actions/transactions/list";
import { TransactionsTable } from "@/components/TransactionsTable";

type SearchParams = Promise<{ [key: string]: string | string[] | undefined }>;

export default async function Home(props: { searchParams: SearchParams }) {
  const searchParams = await props.searchParams;

  const now = new Date();
  const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
  const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);

  const defaultStart = firstDay.toISOString().split('T')[0];
  const defaultEnd = lastDay.toISOString().split('T')[0];

  const startDate = typeof searchParams.startDate === 'string' ? searchParams.startDate : defaultStart;
  const endDate = typeof searchParams.endDate === 'string' ? searchParams.endDate : defaultEnd;

  const { data: transactions, error } = await listTransactions({ 
    page: 1, 
    limit: 10,
    startDate,
    endDate
  });

  return (
    <div className="flex flex-col flex-1 items-center bg-zinc-50 font-sans min-h-screen pb-16">
      <main className="w-full max-w-4xl p-8">
        <div className="flex justify-between items-center mb-8">
          <form method="GET" action="/" className="flex flex-row gap-4">
            <input 
              type="date" 
              name="startDate"
              defaultValue={startDate} 
              style={{ colorScheme: "light" }}
              className="border border-[#121212] text-[#121212] bg-white rounded-lg h-10 px-4 w-[180px]" 
            />
            <input 
              type="date" 
              name="endDate"
              defaultValue={endDate} 
              style={{ colorScheme: "light" }}
              className="border border-[#121212] text-[#121212] bg-white rounded-lg h-10 px-4 w-[180px]" 
            />
            <button 
              type="submit"
              className="bg-[#121212] text-zinc-50 px-4 py-1 rounded-lg hover:bg-zinc-800 transition"
            >
              SEARCH
            </button>
          </form>
          
          <nav className="flex flex-row gap-4">
            <button className="border border-[#121212] text-[#121212] px-4 py-1 rounded-lg hover:bg-zinc-200 transition">IMPORT</button>
            <button className="border border-[#121212] text-[#121212] px-4 py-1 rounded-lg hover:bg-zinc-200 transition">EXPORT</button>
          </nav>
        </div>

        <div className="overflow-x-auto w-full">
          {error ? (
            <div className="text-red-600 font-medium text-center">Failed to load transactions: {error}</div>
          ) : (
            <TransactionsTable 
              key={`${startDate}-${endDate}`}
              initialTransactions={transactions || []} 
              startDate={startDate}
              endDate={endDate}
            />
          )}
        </div>
      </main>
    </div>
  );
}
