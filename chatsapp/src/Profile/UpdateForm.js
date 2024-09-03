import { useState, useEffect } from 'react';
import { useNavigate } from "react-router-dom";
import { useAuth0 } from '@auth0/auth0-react';
import './UpdateProfileForm.css';

const domain = process.env.REACT_APP_AUTH0_DOMAIN;
const audience = `https://${domain}/api/v2/`;

const UpdateProfileForm = () => {
    const navigate = useNavigate();
    const { user, getAccessTokenSilently } = useAuth0();
    const [formData, setFormData] = useState({
        name: '',    birthday: '',    phone: '',
    });

    const handleChange = (e) => {
      const { name, value } = e.target;
      setFormData({...formData, [name]: value,});
    };
    
    useEffect(() => {
        const url = `${audience}users/${user.sub}`;
        getAccessTokenSilently()
        .then(token => fetch(`${url}?fields=user_metadata&include_fields=true`, {
            method: "GET",
            cache: "no-cache",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`,
            }
        }))
        .then(response => response.json())
        .then(data => setFormData(data.user_metadata))
        .catch(err => console.error(toString(err)));
    }, [getAccessTokenSilently, user]);

    const handleSubmit = async (e) => {
        e.preventDefault();

        const birthdayDate = new Date(formData.birthday);
        const today = new Date();
        if (birthdayDate >= today) {
          alert('Birthday must be in the past.');
          return false;
        }

        const token = await getAccessTokenSilently();
        const updatedMetadata = {
          user_metadata: formData,
        };

        const url = `${audience}users/${user.sub}`;
        const response = await fetch(url, {
            method: "PATCH",
            cache: "no-cache",
            headers: {
              "Content-Type": "application/json",
              "Authorization": `Bearer ${token}`,
            },
            body: JSON.stringify(updatedMetadata)
        });

        if (!response.ok) {
          alert("Unsuccessful updated :(");
          return;
        }
        alert("Successful updated :D");
        navigate("/profile");
    };

    return (
      <form className="update-profile-form" onSubmit={handleSubmit}>
          <div className="form-group">
              <label htmlFor="name">Name:</label>
              <input 
                type="text" id="name" name="name" 
                value={formData?.name ?? ""}
                onChange={handleChange}
                minLength="2" maxLength="50"
                pattern="[A-Za-z\s]+" required
                title="Name should only contain letters and spaces"
              />
          </div>
          <div className="form-group">
              <label htmlFor="birthday">Date of Birth:</label>
              <input
                type="date" id="birthday" name="birthday"
                value={formData?.birthday ?? ""} required
                onChange={handleChange}
              />
          </div>
          <div className="form-group">
              <label htmlFor="phone">Phone Number:</label>
              <input
                type="tel" id="phone" name="phone"
                value={formData?.phone ?? ""} 
                onChange={handleChange}
                pattern="[0-9]{10}" required
                minLength="10" maxLength="10"
                title="Telephone number should be a 10-digit number"
              />
          </div>
          <button type="submit">Update Profile</button>
      </form>
    );
};

export default UpdateProfileForm;