// ContactLayout.test.js

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import ContactLayout from './index.js';

const mockContacts = [
  { id: 1, name: 'John Doe', picture: 'john.jpg', lastMessage: 'Hello' },
  { id: 2, name: 'Jane Smith', picture: 'jane.jpg', lastMessage: 'Hi' },
];

const mockOnClick = jest.fn();

describe('ContactLayout', () => {
  test('renders contact cards correctly', () => {
    render(<ContactLayout contact={mockContacts} onClick={mockOnClick} />);
    const contactNames = mockContacts.map(contact => contact.name);

    contactNames.forEach(name => {
      expect(screen.getByText(name)).toBeInTheDocument();
    });
  });

  test('renders input field correctly', () => {
    render(<ContactLayout contact={mockContacts} onClick={mockOnClick} />);
    expect(screen.getByPlaceholderText('Search your contact here...')).toBeInTheDocument();
  });

  test('calls onClick when a contact card is clicked', () => {
    render(<ContactLayout contact={mockContacts} onClick={mockOnClick} />);
    const contactCard = screen.getByText(mockContacts[0].name);
    
    fireEvent.click(contactCard);
    expect(mockOnClick).toHaveBeenCalled();
  });

  test('updates input value when typing in the search field', () => {
    render(<ContactLayout contact={mockContacts} onClick={mockOnClick} />);
    const inputElement = screen.getByPlaceholderText('Search your contact here...');
    
    fireEvent.change(inputElement, { target: { value: 'John' } });
    expect(inputElement.value).toBe('John');
  });
});