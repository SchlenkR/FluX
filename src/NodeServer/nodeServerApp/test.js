
// // const http = require('http');

// // const update = async () => {
// //     try {
// //         const response = await http.get('http://www.google.de');
// //         console.log(response.bo);
// //     }
// //     catch (error) {
// //         console.log(error);
// //     }
// // }

// // update();


// const fetch = require('node-fetch');

// const update = () => {

//     // fetch('https://github.com/')
//     //     .then(res => res.text())
//     //     .then(body => console.log(body));

//     // fetch('https://api.github.com/users/github')
//     //     .catch(err => console.error(err))
//     //     .then(res => res.json())
//     //     .then(json => console.log(json));

//     fetch('https://httpbin.org/post', { method: 'POST' })
//         .catch(err => console.error(err))
//         .then(res => res.json()) // expecting a json response
//         .then(json => console.log(json));
// }

// update();

let arr = [113, 61, 10, 215, 163, 176, 44, 64]
let res = new Float64Array(new Uint8Array(arr).buffer)
console.log(res.length);
