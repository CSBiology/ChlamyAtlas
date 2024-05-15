/** @type {import('tailwindcss').Config} */
module.exports = {
    mode: "jit",
    content: [
        "./index.html",
        "./**/*.{fs,js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {},
    },
    plugins: [
        require("@tailwindcss/typography"), require('daisyui'),
    ],
    daisyui: {
        themes: [
            {
                light: {
                    ...require("daisyui/src/theming/themes")["light"],
                    primary: "#44546a",
                    secondary: "#ed7d31",
                    warning: "#ffc000",
                    error: "#C21F3A",
                    success: "#70ad47",
                    info: "#4472c4"
                },
            },
        ],
    }
}
