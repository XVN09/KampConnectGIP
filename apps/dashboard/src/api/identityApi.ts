import type { Tenant } from './types';

export async function getMyTenants(accessToken : string) : Promise<Tenant[]> {
    // Haal de lijst van tenants op waar de huidige gebruiker toegang toe heeft
    const response = await fetch(`${import.meta.env.VITE_IDENTITY_API_URL}/api/me/tenants`, {
        headers: {
            Authorization: `Bearer ${accessToken}`
        }
    });
    
    // Controleer of de response succesvol was
    if(!response.ok) {
        throw new Error(`Failed to fetch tenants: ${response.statusText} (${response.status})`);
    }
    // Parse de response als JSON en geef terug als een lijst van tenants
    return response.json();
}