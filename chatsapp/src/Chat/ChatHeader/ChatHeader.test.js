import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import ChatHeader from './index.js';

describe('ChatHeader', () => {
  test('renders current contact name correctly', () => {
    const mockCurrentContact = 'John Doe';
    render(<ChatHeader currentContact={mockCurrentContact} />);
    
    const contactName = screen.getByRole('heading', { name: /john doe/i });
    expect(contactName).toBeInTheDocument();
  });
});
