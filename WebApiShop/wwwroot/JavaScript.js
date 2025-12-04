const seeNewUser = document.querySelector(".newUserLink")
const newUser = document.querySelector(".newUser")

seeNewUser.addEventListener("click", e => {
    newUser.style.display = "flex";
})

const getNewUser = () => {
    const user = {
        userName: document.querySelector(".newUserName").value,
        firstName: document.querySelector(".newUserfName").value,
        lastName: document.querySelector(".newUserlName").value,
        password: document.querySelector(".newPassword").value
    };
    return user;
}

async function login() {
    const existUser = getExistsUser();
    try {
        const postResponse = await fetch('/api/users/login', {
            method: 'POST',
            headers: {
                'Content-type': 'application/json'
            },
            body: JSON.stringify(existUser)
        });
        if (!postResponse.ok) {
            throw new Error(`error😢! status of error: ${postResponse.status}`);
        }
        if (postResponse.status == 204) {
            alert("User name dosen't exist!")
        }
        else {
            const dataPost = await postResponse.json();
            sessionStorage.setItem("User", JSON.stringify(dataPost))
            window.location.href = "update.html"
        }
    }
    catch (e) {
        console.log(e)
    }
}

const getExistsUser = () => {
    const user = {
        userName: document.querySelector(".oldUserName").value,
        Password: document.querySelector(".oldPassword").value
    }
    return user;
}

async function addUser() {
    const newUser2 = getNewUser();

    console.log(newUser2);
    try {
        const postResponse = await fetch('api/users', {
            method: 'POST',
            headers: {
                'Content-type': 'application/json'
            },
            body: JSON.stringify(newUser2)
        });
        if (!postResponse.ok) {
            alert("bad response")
            return;
        }
        const data = await postResponse;
        alert("You're inside!");
    }
    catch (e) {
        console.log(e)
    }
}



async function passwordHardness() {
    const password = document.querySelector(".newPassword").value
    try {
        const postResponse = await fetch('api/password', {
            method: 'POST',
            headers: {
                'Content-type': 'application/json'
            },
            body: JSON.stringify(password)
        });
        const data = await postResponse.json();
        //console.log(data)
        alert(`the password hardness is ${data.level}`)
        const pb = document.querySelector(".pb")
        pb.value = data.level
    }
    catch (e) {
        console.log(e)
    }
}