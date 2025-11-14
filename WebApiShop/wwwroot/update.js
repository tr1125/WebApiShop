const message = document.querySelector(".msg")
let oldUser = JSON.parse(sessionStorage.getItem("User"))

message.textContent = `hello ${oldUser.fName}, would you like to update your details?`

const getDetails = () => {
    const updatedUser = {
        userName: document.querySelector(".userName").value || oldUser.userName,
        FName: document.querySelector(".userfName").value || oldUser.fName,
        LName: document.querySelector(".userlName").value || oldUser.lName,
        password: document.querySelector(".password").value || oldUser.password,
        userId: oldUser.userId
    }
    return updatedUser
}

const updateDetails = async () => {
    const updatedUser = getDetails();
    try {
        const postResponse = await fetch(`/api/users/${oldUser.userId}`, {
            method: 'PUT',
            headers: {
                'Content-type': 'application/json'
            },
            body: JSON.stringify(updatedUser)
        });

        const password = updatedUser.password
        const postResponse2 = await fetch('api/password', {
            method: 'POST',
            headers: {
                'Content-type': 'application/json'
            },
            body: JSON.stringify(password)
        });

        if (!postResponse.ok) {
            alert("something went wrong")
            throw new Error(`error😢! status of error: ${postResponse.status}`);
        }
        if (!postResponse2.ok) {
            alert("something went wrong")
            throw new Error(`error😢! status of error: ${postResponse2.status}`);
        }
        sessionStorage.setItem("User", JSON.stringify(updatedUser))
        alert("details updated succesfully")
    }
    catch (e) {
        console.log(e)
    }
}