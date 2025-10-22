/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx,ts,tsx}'],
  theme: {
    extend: {
      boxShadow: {
        royal: '0 0 20px rgba(212, 166, 79, 0.15), 0 0 40px rgba(212, 166, 79, 0.05)',
      },
    },
  },
  plugins: [],
}
