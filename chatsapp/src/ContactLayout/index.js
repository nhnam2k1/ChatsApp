import { useState } from 'react';
import ContactCard from '../Components/ContactCard';
import './index.css';

const ContactLayout = ({contact, onClick}) => {
    const [inputContact, setInputContact] = useState('');

    const contactCards = contact.map((contact) => {
        let updated = contact;
        updated.lastMessage = "";
        updated.profilePicture = updated.picture;
        return <ContactCard contact={updated} callback={onClick}
                            key={contact.id}/>;
    });
                        
    return (
        <div className="contact-container">
            <div className="contact-list">
                {contactCards}
            </div>
            <div className="chat-input">
                <input 
                    type="text" value={inputContact}
                    placeholder="Search your contact here..."  
                    minLength="2" maxLength="50"
                    onChange={(e) => setInputContact(e.target.value)} 
                />
            </div>
        </div>
    );
};

export default ContactLayout;