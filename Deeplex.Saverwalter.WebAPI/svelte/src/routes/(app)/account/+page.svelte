<script lang="ts">
    import { WalterHeader } from '$walter/components';
    import { WalterToastContent } from '$walter/lib';
    import { walter_post } from '$walter/services/requests';
    import { addToast } from '$walter/store';
    import {
        Content,
        PasswordInput,
        Button,
        Tile,
        Accordion,
        AccordionItem
    } from 'carbon-components-svelte';

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

        const response = await walter_post('/api/account/update-password', {
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

<WalterHeader title="Nutzereinstellungen" />

<Content>
    <Accordion align="start" style="max-width: 40em">
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
        <AccordionItem title="Nutzernamen ändern">
            <!-- TODO -->
            <Tile light>Noch nicht implementiert.</Tile>
        </AccordionItem>
        <AccordionItem title="Person auswählen">
            <!-- TODO -->
            <Tile light>Noch nicht implementiert.</Tile>
        </AccordionItem>
    </Accordion>
</Content>
