

const seeNewUser = document.querySelector(".newUserLink")
const newUser = document.querySelector(".newUser")

seeNewUser.addEventListener("click", e => {
    newUser.style.display = "flex";
})

const getNewUser = () => {
    const user = {
        userName: document.querySelector(".newUserName").value,
        fName: document.querySelector(".newUserfName").value,
        lName: document.querySelector(".newUserlName").value,
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
    const newUser = getNewUser();
    const postResponse = await fetch('api/users', {
        method: 'POST',
        headers: {
            'Content-type': 'application/json'
        },
        body: JSON.stringify(newUser)
    });


    
    if (!postResponse.ok) {
        alert("bad response")
        return;
    }
    const data = await postResponse;
    alert("You're inside!");

}



async function passwordHardness() {
    const password = document.querySelector(".newPassword").value
    const postResponse1 = await fetch('api/password', {
        method: 'POST',
        headers: {
            'Content-type': 'application/json'
        },
        body: JSON.stringify(password)
    });
    
    
    const data = await postResponse1.json();
    console.log(data)
    alert(`the password hardness is ${data.level}`)
    const pb = document.querySelector(".pb")
    pb.value = data.level
    //if (!postResponse.ok) {
    //    alert("choose another password")
    //    return;
    //}
    ////const data = await postResponse;
    //alert("You're inside!");

}