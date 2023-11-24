<script lang="ts">
    import { Accordion, AccordionItem, Button, PasswordInput, Row } from "carbon-components-svelte";
    import WalterTextInput from "../elements/WalterTextInput.svelte";
    import { WalterToastContent, type WalterAccountEntry } from "$walter/lib";
    import WalterMultiSelect from "../elements/WalterMultiSelect.svelte";
    import { walter_post, walter_selection } from "$walter/services/requests";
    import { addToast } from "$walter/store";

    export let entry: Partial<WalterAccountEntry> = {};
    export let fetchImpl: typeof fetch;

    const kontakte = walter_selection.kontakte(fetchImpl);

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


<Row>
    <WalterTextInput required labelText="Nutzername" value={entry.username} />
</Row>
<Row>
    <WalterTextInput required labelText="Anzeigename" bind:value={entry.name} />
</Row>

<Row>
    <WalterMultiSelect
        titleText="Verlinkte Kontakte"
        value={entry.kontakte}
        entries={kontakte}/>
</Row>

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
