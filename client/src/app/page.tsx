
export default function Home() {
  return (
    <div className="flex flex-col flex-1 items-center justify-center bg-zinc-50 font-sans">
      <main className="w-[100vw] h-[100vh] p-8">
        <div className="flex justify-between">
          <nav className="flex flex-row gap-4">
            <input type="text" placeholder="Start Date" className="border border-[#121212] rounded-lg h-10 placeholder-[#121212] px-4" />
            <input type="text" placeholder="End Date" className="border border-[#121212] rounded-lg h-10 placeholder-[#121212] px-4" />
          </nav>
          
          <nav className="flex flex-row gap-4">
            <button className="border border-[#121212] text-[#121212] px-4 py-1 rounded-lg">IMPORT</button>
            <button className="border border-[#121212] text-[#121212] px-4 py-1 rounded-lg">EXPORT</button>
          </nav>
        </div>
      </main>
    </div>
  );
}
