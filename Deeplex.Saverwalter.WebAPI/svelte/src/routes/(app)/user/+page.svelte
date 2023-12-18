<script lang="ts">
    import { WalterGrid, WalterHeaderDetail } from '$walter/components';
    import WalterAccount from '$walter/components/details/WalterAccount.svelte';
    import { WalterToastContent } from '$walter/lib';
    import { walter_post } from '$walter/services/requests';
    import { addToast } from '$walter/store';
    import {
        Accordion,
        AccordionItem,
        Button,
        PasswordInput
    } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    export let data: PageData;
    const title = `Nutzereinstellungen`;

    let old_password = '';
    let new_password_1 = '';
    let new_password_2 = '';
    let old_password_invalid = false;
    let new_password_invalid = false;

    async function check_and_update_password() {
        if (new_password_1 !== new_password_2) {
            new_password_invalid = true;
            return;
        }

        const ChangeToast = new WalterToastContent(
            'Passwort erfolgreich geändert',
            'Passwort konnte nicht geändert werden'
        );

        const response = await walter_post('/api/user/update-password', {
            OldPassword: old_password,
            NewPassword: new_password_1
        });

        addToast(ChangeToast, response.status === 200);

        if (response.status === 400) {
            old_password_invalid = true;
        } else if (response.status === 200) {
            old_password = '';
            new_password_1 = '';
            new_password_2 = '';
            new_password_invalid = false;
            old_password_invalid = false;
        }
    }
</script>

<WalterHeaderDetail entry={data.entry} apiURL={data.apiURL} {title} />

<WalterGrid>
    <WalterAccount entry={data.entry} fetchImpl={data.fetchImpl} />

    <Accordion align="start">
        <AccordionItem title="Passwort ändern">
            <PasswordInput
                labelText="Aktuelles Passwort eingeben"
                bind:invalid={old_password_invalid}
                invalidText="Passwort ist nicht korrekt"
                bind:value={old_password}
            />
            <PasswordInput
                labelText="Neues Passwort eingeben"
                bind:invalid={new_password_invalid}
                invalidText="Passwörter stimmen nicht überein"
                bind:value={new_password_1}
            />
            <PasswordInput
                labelText="Neues Passwort wiederholen"
                bind:invalid={new_password_invalid}
                invalidText="Passwörter stimmen nicht überein"
                bind:value={new_password_2}
            />
            <div style="display: flex; justify-content:center;">
                <Button on:click={check_and_update_password}
                    >Passwort ändern</Button
                >
            </div>
            <div style="height: 2em" />
        </AccordionItem>
    </Accordion>
</WalterGrid>
