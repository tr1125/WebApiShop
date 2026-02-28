/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      backdropBlur: {
        md: '12px',
      },
      colors: {
        cream: '#F5E6E0',
        forest: '#2C5530',
      }
    },
  },
  plugins: [],
}
